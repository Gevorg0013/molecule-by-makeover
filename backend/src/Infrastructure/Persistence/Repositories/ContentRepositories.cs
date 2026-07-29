using Microsoft.EntityFrameworkCore;
using MoleculeByMakeover.Domain.Entities;
using MoleculeByMakeover.Domain.Interfaces;

namespace MoleculeByMakeover.Infrastructure.Persistence.Repositories;

public class BlogPostRepository(AppDbContext context) : Repository<BlogPost>(context), IBlogPostRepository
{
    public async Task<BlogPost?> GetBySlugAsync(string slug, int languageId, CancellationToken ct = default) =>
        await DbSet.Include(b => b.Translations)
            .FirstOrDefaultAsync(b => b.Translations.Any(t => t.Slug == slug && t.LanguageId == languageId), ct);
}

public class PageRepository(AppDbContext context) : Repository<Page>(context), IPageRepository
{
    public async Task<Page?> GetByKeyAsync(string key, CancellationToken ct = default) =>
        await DbSet.Include(p => p.Translations).FirstOrDefaultAsync(p => p.Key == key, ct);
}

public class BannerRepository(AppDbContext context) : Repository<Banner>(context), IBannerRepository
{
    public async Task<List<Banner>> GetActiveAsync(CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;
        return await DbSet.Include(b => b.Translations)
            .Where(b => b.IsActive && (b.StartsAt == null || b.StartsAt <= now) && (b.EndsAt == null || b.EndsAt >= now))
            .OrderBy(b => b.SortOrder)
            .ToListAsync(ct);
    }
}

public class GalleryImageRepository(AppDbContext context) : Repository<GalleryImage>(context), IGalleryImageRepository;
