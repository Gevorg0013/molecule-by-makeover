using MoleculeByMakeover.Domain.Entities;

namespace MoleculeByMakeover.Domain.Interfaces;

public interface IBlogPostRepository : IRepository<BlogPost>
{
    Task<BlogPost?> GetBySlugAsync(string slug, int languageId, CancellationToken ct = default);
}

public interface IPageRepository : IRepository<Page>
{
    Task<Page?> GetByKeyAsync(string key, CancellationToken ct = default);
}

public interface IBannerRepository : IRepository<Banner>
{
    Task<List<Banner>> GetActiveAsync(CancellationToken ct = default);
}

public interface IGalleryImageRepository : IRepository<GalleryImage>
{
}

public interface ISettingRepository
{
    Task<string?> GetValueAsync(string key, CancellationToken ct = default);
    Task<List<Setting>> GetByGroupAsync(string group, CancellationToken ct = default);
    Task UpsertAsync(string key, string? value, string group, CancellationToken ct = default);
}

public interface ILanguageRepository : IRepository<Language>
{
    Task<Language?> GetByCodeAsync(string code, CancellationToken ct = default);
    Task<Language> GetDefaultAsync(CancellationToken ct = default);
}

public interface INewsletterSubscriberRepository : IRepository<NewsletterSubscriber>
{
    Task<bool> ExistsAsync(string email, CancellationToken ct = default);
}
