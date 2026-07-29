using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MoleculeByMakeover.Application.Common.Interfaces;
using MoleculeByMakeover.Domain.Entities;
using MoleculeByMakeover.Domain.Interfaces;
using MoleculeByMakeover.Shared.Common;
using MoleculeByMakeover.Shared.Exceptions;

namespace MoleculeByMakeover.Application.Features.Catalog;

public class ProductService(
    IUnitOfWork unitOfWork,
    ICurrentLanguageService currentLanguage,
    IValidator<ProductUpsertRequest> validator) : IProductService
{
    public async Task<PaginatedList<ProductListItemDto>> SearchAsync(ProductQueryParameters query, CancellationToken ct = default)
    {
        var languageId = currentLanguage.LanguageId;
        var q = unitOfWork.Products.QueryWithTranslations().Where(p => p.IsActive && !p.IsDeleted);

        if (!string.IsNullOrWhiteSpace(query.CategorySlug))
            q = q.Where(p => p.Category.Translations.Any(t => t.Slug == query.CategorySlug));

        if (query.MinPrice is not null) q = q.Where(p => p.Price >= query.MinPrice);
        if (query.MaxPrice is not null) q = q.Where(p => p.Price <= query.MaxPrice);
        if (!string.IsNullOrWhiteSpace(query.Brand)) q = q.Where(p => p.Brand == query.Brand);
        if (!string.IsNullOrWhiteSpace(query.Tag)) q = q.Where(p => p.ProductTags.Any(pt => pt.Tag.Name == query.Tag));
        if (query.FeaturedOnly == true) q = q.Where(p => p.IsFeatured);
        if (query.BestSellerOnly == true) q = q.Where(p => p.IsBestSeller);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLower();
            q = q.Where(p => p.Translations.Any(t =>
                t.LanguageId == languageId &&
                (t.Name.ToLower().Contains(term) || (t.ShortDescription != null && t.ShortDescription.ToLower().Contains(term)))));
        }

        q = query.Sort switch
        {
            ProductSort.PriceAsc => q.OrderBy(p => p.Price),
            ProductSort.PriceDesc => q.OrderByDescending(p => p.Price),
            ProductSort.BestSelling => q.OrderByDescending(p => p.IsBestSeller).ThenByDescending(p => p.CreatedAt),
            _ => q.OrderByDescending(p => p.CreatedAt)
        };

        var totalCount = await q.CountAsync(ct);
        var items = await q.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(ct);

        return PaginatedList<ProductListItemDto>.Create(
            items.Select(ToListItemDto).ToList(), totalCount, query.Page, query.PageSize);
    }

    public async Task<ProductDetailDto> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        var product = await unitOfWork.Products.GetBySlugAsync(slug, currentLanguage.LanguageId, ct)
            ?? throw new NotFoundException(nameof(Product), slug);

        return ToDetailDto(product);
    }

    public async Task<List<ProductListItemDto>> GetRelatedAsync(Guid productId, int take = 8, CancellationToken ct = default)
    {
        var product = await unitOfWork.Products.GetByIdAsync(productId, ct)
            ?? throw new NotFoundException(nameof(Product), productId);

        var related = await unitOfWork.Products.QueryWithTranslations()
            .Where(p => p.Id != productId && p.CategoryId == product.CategoryId && p.IsActive && !p.IsDeleted)
            .OrderByDescending(p => p.IsBestSeller)
            .Take(take)
            .ToListAsync(ct);

        return related.Select(ToListItemDto).ToList();
    }

    public async Task<List<ProductAdminDto>> GetAllAdminAsync(CancellationToken ct = default)
    {
        var products = await unitOfWork.Products.QueryWithTranslations().Where(p => !p.IsDeleted).ToListAsync(ct);
        return products.Select(ToAdminDto).ToList();
    }

    public async Task<ProductAdminDto> GetByIdAdminAsync(Guid id, CancellationToken ct = default)
    {
        var product = await unitOfWork.Products.QueryWithTranslations().FirstOrDefaultAsync(p => p.Id == id, ct)
            ?? throw new NotFoundException(nameof(Product), id);

        return ToAdminDto(product);
    }

    public async Task<Guid> CreateAsync(ProductUpsertRequest request, CancellationToken ct = default)
    {
        await validator.ValidateAndThrowAsync(request, ct);

        var product = new Product
        {
            Sku = request.Sku,
            CategoryId = request.CategoryId,
            Price = request.Price,
            DiscountType = request.DiscountType,
            DiscountValue = request.DiscountValue,
            Stock = request.Stock,
            MainImageUrl = request.MainImageUrl,
            Brand = request.Brand,
            IsFeatured = request.IsFeatured,
            IsBestSeller = request.IsBestSeller,
            IsActive = request.IsActive,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await ApplyTranslationsAsync(product, request.Translations, ct);
        await ApplyTagsAsync(product, request.Tags, ct);

        await unitOfWork.Products.AddAsync(product, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return product.Id;
    }

    public async Task UpdateAsync(Guid id, ProductUpsertRequest request, CancellationToken ct = default)
    {
        await validator.ValidateAndThrowAsync(request, ct);

        var product = await unitOfWork.Products.QueryWithTranslations().FirstOrDefaultAsync(p => p.Id == id, ct)
            ?? throw new NotFoundException(nameof(Product), id);

        product.Sku = request.Sku;
        product.CategoryId = request.CategoryId;
        product.Price = request.Price;
        product.DiscountType = request.DiscountType;
        product.DiscountValue = request.DiscountValue;
        product.Stock = request.Stock;
        product.MainImageUrl = request.MainImageUrl;
        product.Brand = request.Brand;
        product.IsFeatured = request.IsFeatured;
        product.IsBestSeller = request.IsBestSeller;
        product.IsActive = request.IsActive;
        product.UpdatedAt = DateTimeOffset.UtcNow;

        product.Translations.Clear();
        await ApplyTranslationsAsync(product, request.Translations, ct);

        await ApplyTagsAsync(product, request.Tags, ct);

        unitOfWork.Products.Update(product);
        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var product = await unitOfWork.Products.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Product), id);

        product.IsDeleted = true;
        product.DeletedAt = DateTimeOffset.UtcNow;
        unitOfWork.Products.Update(product);
        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task<ProductImageDto> AddImageAsync(Guid productId, string url, string? altText, CancellationToken ct = default)
    {
        var product = await unitOfWork.Products.QueryWithTranslations().FirstOrDefaultAsync(p => p.Id == productId, ct)
            ?? throw new NotFoundException(nameof(Product), productId);

        var image = new ProductImage
        {
            ProductId = productId,
            Url = url,
            AltText = altText,
            SortOrder = product.Images.Count,
            IsPrimary = product.Images.Count == 0,
            CreatedAt = DateTimeOffset.UtcNow
        };

        product.Images.Add(image);
        unitOfWork.TrackNew(image);
        await unitOfWork.SaveChangesAsync(ct);

        return new ProductImageDto(image.Id, image.Url, image.AltText, image.SortOrder, image.IsPrimary);
    }

    public async Task RemoveImageAsync(Guid productId, Guid imageId, CancellationToken ct = default)
    {
        var product = await unitOfWork.Products.QueryWithTranslations().FirstOrDefaultAsync(p => p.Id == productId, ct)
            ?? throw new NotFoundException(nameof(Product), productId);

        var image = product.Images.FirstOrDefault(i => i.Id == imageId)
            ?? throw new NotFoundException(nameof(ProductImage), imageId);

        product.Images.Remove(image);
        unitOfWork.Products.Update(product);
        await unitOfWork.SaveChangesAsync(ct);
    }

    private async Task ApplyTranslationsAsync(Product product, List<ProductTranslationInput> translations, CancellationToken ct)
    {
        foreach (var t in translations)
        {
            var language = await unitOfWork.Languages.GetByCodeAsync(t.LanguageCode, ct)
                ?? throw new BadRequestException($"Unknown language code '{t.LanguageCode}'.");

            var translation = new ProductTranslation
            {
                LanguageId = language.Id,
                Name = t.Name,
                Slug = t.Slug,
                ShortDescription = t.ShortDescription,
                Description = t.Description,
                Ingredients = t.Ingredients,
                UsageInstructions = t.UsageInstructions,
                Benefits = t.Benefits,
                MetaTitle = t.MetaTitle,
                MetaDescription = t.MetaDescription,
                CreatedAt = DateTimeOffset.UtcNow
            };
            product.Translations.Add(translation);
            unitOfWork.TrackNew(translation);
        }
    }

    /// <summary>
    /// Diffs against the currently-tracked tags rather than clear-and-recreate: ProductTag has no
    /// independent BaseEntity.Id (its key is the ProductId/TagId pair), so removing and re-adding
    /// the same tag in one SaveChanges would try to track two instances under the same key.
    /// </summary>
    private async Task ApplyTagsAsync(Product product, List<string> tagNames, CancellationToken ct)
    {
        var desiredNames = tagNames.Select(n => n.Trim().ToLowerInvariant()).Distinct().ToList();

        foreach (var toRemove in product.ProductTags.Where(pt => !desiredNames.Contains(pt.Tag.Name)).ToList())
            product.ProductTags.Remove(toRemove);

        if (desiredNames.Count == 0) return;

        var tags = await unitOfWork.Tags.GetOrCreateAsync(desiredNames, ct);
        var existingTagIds = product.ProductTags.Select(pt => pt.TagId).ToHashSet();

        foreach (var tag in tags.Where(t => !existingTagIds.Contains(t.Id)))
        {
            var productTag = new ProductTag { ProductId = product.Id, TagId = tag.Id };
            product.ProductTags.Add(productTag);
            unitOfWork.TrackNew(productTag);
        }
    }

    private ProductListItemDto ToListItemDto(Product product)
    {
        var translation = ResolveTranslation(product.Translations);
        var categoryTranslation = ResolveTranslation(product.Category.Translations);
        var approvedReviews = product.Reviews.Where(r => r.IsApproved).ToList();

        return new ProductListItemDto(
            product.Id, product.Sku, translation?.Name ?? string.Empty, translation?.Slug ?? string.Empty,
            translation?.ShortDescription, product.Price, product.FinalPrice, product.DiscountType, product.DiscountValue,
            product.MainImageUrl, product.Stock, product.Brand, product.IsFeatured, product.IsBestSeller,
            product.CategoryId, categoryTranslation?.Name ?? string.Empty,
            product.ProductTags.Select(pt => pt.Tag.Name).ToList(),
            approvedReviews.Count > 0 ? approvedReviews.Average(r => r.Rating) : 0,
            approvedReviews.Count);
    }

    private ProductDetailDto ToDetailDto(Product product)
    {
        var translation = ResolveTranslation(product.Translations);
        var categoryTranslation = ResolveTranslation(product.Category.Translations);
        var approvedReviews = product.Reviews.Where(r => r.IsApproved).ToList();

        return new ProductDetailDto(
            product.Id, product.Sku, translation?.Name ?? string.Empty, translation?.Slug ?? string.Empty,
            translation?.ShortDescription, translation?.Description, translation?.Ingredients, translation?.UsageInstructions,
            translation?.Benefits, product.Price, product.FinalPrice, product.DiscountType, product.DiscountValue,
            product.MainImageUrl, product.Stock, product.Brand, product.CategoryId, categoryTranslation?.Name ?? string.Empty,
            product.ProductTags.Select(pt => pt.Tag.Name).ToList(),
            product.Images.OrderBy(i => i.SortOrder).Select(i => new ProductImageDto(i.Id, i.Url, i.AltText, i.SortOrder, i.IsPrimary)).ToList(),
            translation?.MetaTitle, translation?.MetaDescription,
            approvedReviews.Count > 0 ? approvedReviews.Average(r => r.Rating) : 0,
            approvedReviews.Count);
    }

    private static ProductAdminDto ToAdminDto(Product product) => new(
        product.Id, product.Sku, product.CategoryId, product.Price, product.DiscountType, product.DiscountValue,
        product.Stock, product.MainImageUrl, product.Brand, product.IsFeatured, product.IsBestSeller, product.IsActive,
        product.ProductTags.Select(pt => pt.Tag.Name).ToList(),
        product.Images.OrderBy(i => i.SortOrder).Select(i => new ProductImageDto(i.Id, i.Url, i.AltText, i.SortOrder, i.IsPrimary)).ToList(),
        product.Translations.Select(t => new ProductTranslationDto(
            t.Id, t.Language.Code, t.Name, t.Slug, t.ShortDescription, t.Description, t.Ingredients,
            t.UsageInstructions, t.Benefits, t.MetaTitle, t.MetaDescription)).ToList());

    private ProductTranslation? ResolveTranslation(IEnumerable<ProductTranslation> translations) =>
        translations.FirstOrDefault(t => t.LanguageId == currentLanguage.LanguageId) ?? translations.FirstOrDefault();

    private CategoryTranslation? ResolveTranslation(IEnumerable<CategoryTranslation> translations) =>
        translations.FirstOrDefault(t => t.LanguageId == currentLanguage.LanguageId) ?? translations.FirstOrDefault();
}
