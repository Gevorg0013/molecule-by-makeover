namespace MoleculeByMakeover.Application.Features.Content;

public interface IPageService
{
    Task<PageDto> GetByKeyAsync(string key, CancellationToken ct = default);

    Task<List<PageAdminDto>> GetAllAdminAsync(CancellationToken ct = default);
    Task<PageAdminDto> GetByIdAdminAsync(Guid id, CancellationToken ct = default);
    Task<Guid> CreateAsync(PageUpsertRequest request, CancellationToken ct = default);
    Task UpdateAsync(Guid id, PageUpsertRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
