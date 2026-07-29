namespace MoleculeByMakeover.Application.Features.Catalog;

public interface ICategoryService
{
    Task<List<CategoryDto>> GetTreeAsync(CancellationToken ct = default);
    Task<CategoryDto> GetBySlugAsync(string slug, CancellationToken ct = default);

    Task<List<CategoryAdminDto>> GetAllAdminAsync(CancellationToken ct = default);
    Task<CategoryAdminDto> GetByIdAdminAsync(Guid id, CancellationToken ct = default);
    Task<Guid> CreateAsync(CategoryUpsertRequest request, CancellationToken ct = default);
    Task UpdateAsync(Guid id, CategoryUpsertRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
