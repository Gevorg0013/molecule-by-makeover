using Microsoft.EntityFrameworkCore;
using MoleculeByMakeover.Domain.Entities;
using MoleculeByMakeover.Domain.Interfaces;

namespace MoleculeByMakeover.Infrastructure.Persistence.Repositories;

public class ProductRepository(AppDbContext context) : Repository<Product>(context), IProductRepository
{
    public IQueryable<Product> QueryWithTranslations() =>
        DbSet
            .Include(p => p.Translations).ThenInclude(t => t.Language)
            .Include(p => p.Images)
            .Include(p => p.ProductTags).ThenInclude(pt => pt.Tag)
            .Include(p => p.Category).ThenInclude(c => c.Translations)
            .Include(p => p.Reviews);

    public async Task<Product?> GetBySlugAsync(string slug, int languageId, CancellationToken ct = default) =>
        await QueryWithTranslations()
            .FirstOrDefaultAsync(p => p.Translations.Any(t => t.Slug == slug && t.LanguageId == languageId), ct);
}

public class CategoryRepository(AppDbContext context) : Repository<Category>(context), ICategoryRepository
{
    public IQueryable<Category> QueryWithTranslations() => DbSet.Include(c => c.Translations).ThenInclude(t => t.Language);

    public async Task<Category?> GetBySlugAsync(string slug, int languageId, CancellationToken ct = default) =>
        await QueryWithTranslations()
            .FirstOrDefaultAsync(c => c.Translations.Any(t => t.Slug == slug && t.LanguageId == languageId), ct);
}

public class TagRepository(AppDbContext context) : Repository<Tag>(context), ITagRepository
{
    public async Task<List<Tag>> GetOrCreateAsync(IEnumerable<string> names, CancellationToken ct = default)
    {
        var normalized = names.Select(n => n.Trim().ToLowerInvariant()).Distinct().ToList();
        var existing = await DbSet.Where(t => normalized.Contains(t.Name)).ToListAsync(ct);

        var missing = normalized.Except(existing.Select(t => t.Name)).ToList();
        var created = missing.Select(name => new Tag { Name = name }).ToList();
        if (created.Count > 0)
            await DbSet.AddRangeAsync(created, ct);

        return [.. existing, .. created];
    }
}
