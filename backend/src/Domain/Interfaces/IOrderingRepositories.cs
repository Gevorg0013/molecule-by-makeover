using MoleculeByMakeover.Domain.Entities;

namespace MoleculeByMakeover.Domain.Interfaces;

public interface ICartRepository : IRepository<Cart>
{
    Task<Cart?> GetActiveByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<Cart?> GetActiveByGuestTokenAsync(string guestToken, CancellationToken ct = default);

    /// <summary>
    /// Explicitly tracks a newly-created CartItem as Added. Required because BaseEntity.Id is
    /// assigned a non-default Guid at construction time, so relying on collection-navigation
    /// fixup alone causes EF Core to treat the new row as an existing one to UPDATE.
    /// </summary>
    Task AddItemAsync(CartItem item, CancellationToken ct = default);
}

public interface ICouponRepository : IRepository<Coupon>
{
    Task<Coupon?> GetByCodeAsync(string code, CancellationToken ct = default);
}

public interface IOrderRepository : IRepository<Order>
{
    Task<Order?> GetByOrderNumberAsync(string orderNumber, CancellationToken ct = default);
    Task<List<Order>> GetPendingExpiredReservationsAsync(DateTimeOffset asOf, CancellationToken ct = default);
}

public interface IWishlistRepository : IRepository<Wishlist>
{
    Task<Wishlist?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Explicitly tracks a newly-created WishlistItem as Added. See ICartRepository.AddItemAsync
    /// for why this is required instead of relying on collection-navigation fixup alone.
    /// </summary>
    Task AddItemAsync(WishlistItem item, CancellationToken ct = default);
}

public interface IReviewRepository : IRepository<Review>
{
    Task<bool> HasUserReviewedAsync(Guid productId, Guid userId, CancellationToken ct = default);
}
