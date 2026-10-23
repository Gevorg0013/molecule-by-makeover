using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using MoleculeByMakeover.Domain.Entities;

namespace MoleculeByMakeover.Infrastructure.Persistence.Seed;

// Brings a database up to the shop's catalog (categories, products, banners, gallery, blog) from
// catalog-snapshot.json, whose images ship in the repo under wwwroot/uploads. Runs once per
// database (recorded in Settings), so products an admin later deletes are not brought back.
// Rows that already exist are kept as they are; only their image is replaced when the file it
// points to is missing, which is what made a fresh clone show broken images.
// No users, orders or other personal data are part of the snapshot.
public static class CatalogSnapshotSeeder
{
    private const string ResourceName = "MoleculeByMakeover.Infrastructure.Persistence.Seed.catalog-snapshot.json";
    private const string MarkerKey = "catalog_snapshot_version";
    private const string MarkerGroup = "system";
    private const string SnapshotVersion = "1";

    public static async Task SeedAsync(AppDbContext context, Dictionary<string, Language> languages, Func<string?, bool> imageExists)
    {
        var marker = await context.Settings.FirstOrDefaultAsync(s => s.Key == MarkerKey);
        if (marker?.Value == SnapshotVersion) return;

        var snapshot = await LoadAsync();

        var categoryIds = await SeedCategoriesAsync(context, snapshot.Categories, languages, imageExists);
        await SeedProductsAsync(context, snapshot.Products, categoryIds, languages, imageExists);
        await SeedBannersAsync(context, snapshot.Banners, languages, imageExists);
        await SeedGalleryAsync(context, snapshot.GalleryImages);
        await SeedBlogPostsAsync(context, snapshot.BlogPosts, languages, imageExists);

        if (marker is null)
            context.Settings.Add(new Setting { Key = MarkerKey, Value = SnapshotVersion, Group = MarkerGroup });
        else
            marker.Value = SnapshotVersion;

        await context.SaveChangesAsync();
    }

    // Snapshot category id -> id of the matching category in this database.
    private static async Task<Dictionary<Guid, Guid>> SeedCategoriesAsync(
        AppDbContext context, List<Category> categories, Dictionary<string, Language> languages, Func<string?, bool> imageExists)
    {
        var ids = new Dictionary<Guid, Guid>();
        var existingCategories = await context.Categories.IgnoreQueryFilters().Include(c => c.Translations).ToListAsync();

        foreach (var category in categories)
        {
            ResolveLanguages(category.Translations, languages, t => t.Language, (t, language) => { t.LanguageId = language.Id; t.Language = language; });

            var existing = existingCategories.FirstOrDefault(c => c.Id == category.Id)
                ?? existingCategories.FirstOrDefault(c => c.Translations.Any(t =>
                    category.Translations.Any(s => s.LanguageId == t.LanguageId && s.Slug == t.Slug)));

            if (existing is not null)
            {
                ids[category.Id] = existing.Id;
                if (!imageExists(existing.ImageUrl) && imageExists(category.ImageUrl))
                    existing.ImageUrl = category.ImageUrl;
                continue;
            }

            if (category.ParentCategoryId is { } parentId)
                category.ParentCategoryId = ids.TryGetValue(parentId, out var mappedParent) ? mappedParent : null;

            context.Categories.Add(category);
            existingCategories.Add(category);
            ids[category.Id] = category.Id;
        }

        return ids;
    }

    private static async Task SeedProductsAsync(
        AppDbContext context, List<Product> products, Dictionary<Guid, Guid> categoryIds,
        Dictionary<string, Language> languages, Func<string?, bool> imageExists)
    {
        var usedSlugs = (await context.ProductTranslations.IgnoreQueryFilters()
                .Select(t => new { t.Slug, t.LanguageId }).ToListAsync())
            .Select(t => (t.Slug, t.LanguageId)).ToHashSet();

        foreach (var product in products)
        {
            ResolveLanguages(product.Translations, languages, t => t.Language, (t, language) => { t.LanguageId = language.Id; t.Language = language; });

            var existing = await context.Products.IgnoreQueryFilters().Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Sku == product.Sku);

            if (existing is not null)
            {
                if (existing.IsDeleted) continue;

                if (!imageExists(existing.MainImageUrl) && imageExists(product.MainImageUrl))
                    existing.MainImageUrl = product.MainImageUrl;

                if (product.Images.Count > 0 && !existing.Images.Any(i => imageExists(i.Url)))
                {
                    context.ProductImages.RemoveRange(existing.Images);
                    foreach (var image in product.Images)
                        context.ProductImages.Add(new ProductImage
                        {
                            ProductId = existing.Id, Url = image.Url, AltText = image.AltText,
                            SortOrder = image.SortOrder, IsPrimary = image.IsPrimary, CreatedAt = DateTimeOffset.UtcNow
                        });
                }
                continue;
            }

            if (!categoryIds.TryGetValue(product.CategoryId, out var categoryId)) continue;
            product.CategoryId = categoryId;

            // A slug taken by another product would violate the unique index; keep the rest.
            product.Translations = product.Translations.Where(t => !usedSlugs.Contains((t.Slug, t.LanguageId))).ToList();
            if (product.Translations.Count == 0) continue;
            foreach (var t in product.Translations) usedSlugs.Add((t.Slug, t.LanguageId));

            context.Products.Add(product);
        }
    }

    private static async Task SeedBannersAsync(
        AppDbContext context, List<Banner> banners, Dictionary<string, Language> languages, Func<string?, bool> imageExists)
    {
        foreach (var banner in banners)
        {
            var existing = await context.Banners.FirstOrDefaultAsync(b => b.Id == banner.Id);
            if (existing is not null)
            {
                if (!imageExists(existing.ImageUrl) && imageExists(banner.ImageUrl))
                    existing.ImageUrl = banner.ImageUrl;
                continue;
            }

            ResolveLanguages(banner.Translations, languages, t => t.Language, (t, language) => { t.LanguageId = language.Id; t.Language = language; });
            context.Banners.Add(banner);
        }
    }

    private static async Task SeedGalleryAsync(AppDbContext context, List<GalleryImage> images)
    {
        var existingUrls = (await context.GalleryImages.Select(g => g.Url).ToListAsync()).ToHashSet();
        foreach (var image in images.Where(i => !existingUrls.Contains(i.Url)))
            context.GalleryImages.Add(image);
    }

    private static async Task SeedBlogPostsAsync(
        AppDbContext context, List<BlogPost> posts, Dictionary<string, Language> languages, Func<string?, bool> imageExists)
    {
        var usedSlugs = (await context.BlogPostTranslations.IgnoreQueryFilters()
                .Select(t => new { t.Slug, t.LanguageId }).ToListAsync())
            .Select(t => (t.Slug, t.LanguageId)).ToHashSet();

        foreach (var post in posts)
        {
            var existing = await context.BlogPosts.IgnoreQueryFilters().FirstOrDefaultAsync(b => b.Id == post.Id);
            if (existing is not null)
            {
                if (!existing.IsDeleted && !imageExists(existing.CoverImageUrl) && imageExists(post.CoverImageUrl))
                    existing.CoverImageUrl = post.CoverImageUrl;
                continue;
            }

            ResolveLanguages(post.Translations, languages, t => t.Language, (t, language) => { t.LanguageId = language.Id; t.Language = language; });
            post.Translations = post.Translations.Where(t => !usedSlugs.Contains((t.Slug, t.LanguageId))).ToList();
            if (post.Translations.Count == 0) continue;
            foreach (var t in post.Translations) usedSlugs.Add((t.Slug, t.LanguageId));

            context.BlogPosts.Add(post);
        }
    }

    // The snapshot names languages by code (ids differ between databases). Translations in a
    // language this database doesn't have are dropped.
    private static void ResolveLanguages<T>(
        ICollection<T> translations, Dictionary<string, Language> languages, Func<T, Language?> snapshotLanguage, Action<T, Language> assign)
    {
        foreach (var translation in translations.ToList())
        {
            var code = snapshotLanguage(translation)?.Code;
            if (code is not null && languages.TryGetValue(code, out var language))
                assign(translation, language);
            else
                translations.Remove(translation);
        }
    }

    private static async Task<CatalogSnapshot> LoadAsync()
    {
        await using var stream = typeof(CatalogSnapshotSeeder).Assembly.GetManifestResourceStream(ResourceName)
            ?? throw new InvalidOperationException($"Embedded resource {ResourceName} not found.");
        return await JsonSerializer.DeserializeAsync<CatalogSnapshot>(stream)
            ?? throw new InvalidOperationException("The catalog snapshot is empty.");
    }

    private sealed class CatalogSnapshot
    {
        public List<Category> Categories { get; set; } = [];
        public List<Product> Products { get; set; } = [];
        public List<Banner> Banners { get; set; } = [];
        public List<GalleryImage> GalleryImages { get; set; } = [];
        public List<BlogPost> BlogPosts { get; set; } = [];
    }
}
