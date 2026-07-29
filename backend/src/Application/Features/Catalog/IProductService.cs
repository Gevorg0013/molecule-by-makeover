using MoleculeByMakeover.Shared.Common;

namespace MoleculeByMakeover.Application.Features.Catalog;

public interface IProductService
{
    Task<PaginatedList<ProductListItemDto>> SearchAsync(ProductQueryParameters query, CancellationToken ct = default);
    Task<ProductDetailDto> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<List<ProductListItemDto>> GetRelatedAsync(Guid productId, int take = 8, CancellationToken ct = default);

    Task<List<ProductAdminDto>> GetAllAdminAsync(CancellationToken ct = default);
    Task<ProductAdminDto> GetByIdAdminAsync(Guid id, CancellationToken ct = default);
    Task<Guid> CreateAsync(ProductUpsertRequest request, CancellationToken ct = default);
    Task UpdateAsync(Guid id, ProductUpsertRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<ProductImageDto> AddImageAsync(Guid productId, string url, string? altText, CancellationToken ct = default);
    Task RemoveImageAsync(Guid productId, Guid imageId, CancellationToken ct = default);
}
