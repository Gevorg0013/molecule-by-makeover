using MoleculeByMakeover.Domain.Entities;

namespace MoleculeByMakeover.Domain.Interfaces;

public interface IProductRepository : IRepository<Product>
{
    IQueryable<Product> QueryWithTranslations();
    Task<Product?> GetBySlugAsync(string slug, int languageId, CancellationToken ct = default);
}

public interface ICategoryRepository : IRepository<Category>
{
    IQueryable<Category> QueryWithTranslations();
    Task<Category?> GetBySlugAsync(string slug, int languageId, CancellationToken ct = default);
}

public interface ITagRepository : IRepository<Tag>
{
    Task<List<Tag>> GetOrCreateAsync(IEnumerable<string> names, CancellationToken ct = default);
}
