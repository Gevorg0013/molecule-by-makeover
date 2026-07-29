namespace MoleculeByMakeover.Application.Features.Wishlists;

public interface IWishlistService
{
    Task<WishlistDto> GetAsync(Guid userId, CancellationToken ct = default);
    Task AddAsync(Guid userId, Guid productId, CancellationToken ct = default);
    Task RemoveAsync(Guid userId, Guid productId, CancellationToken ct = default);
}
