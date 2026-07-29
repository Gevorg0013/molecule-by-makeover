using MoleculeByMakeover.Shared.Common;

namespace MoleculeByMakeover.Application.Features.Content;

public interface IBlogService
{
    Task<PaginatedList<BlogPostListItemDto>> GetPublishedAsync(int page, int pageSize, CancellationToken ct = default);
    Task<BlogPostDetailDto> GetBySlugAsync(string slug, CancellationToken ct = default);

    Task<List<BlogPostAdminDto>> GetAllAdminAsync(CancellationToken ct = default);
    Task<BlogPostAdminDto> GetByIdAdminAsync(Guid id, CancellationToken ct = default);
    Task<Guid> CreateAsync(Guid authorId, BlogPostUpsertRequest request, CancellationToken ct = default);
    Task UpdateAsync(Guid id, BlogPostUpsertRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
