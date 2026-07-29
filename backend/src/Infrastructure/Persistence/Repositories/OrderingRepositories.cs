using Microsoft.EntityFrameworkCore;
using MoleculeByMakeover.Domain.Entities;
using MoleculeByMakeover.Domain.Enums;
using MoleculeByMakeover.Domain.Interfaces;

namespace MoleculeByMakeover.Infrastructure.Persistence.Repositories;

public class CartRepository(AppDbContext context) : Repository<Cart>(context), ICartRepository
{
    private IQueryable<Cart> QueryWithDetails() =>
        DbSet.Include(c => c.Items).ThenInclude(i => i.Product).ThenInclude(p => p.Translations)
            .Include(c => c.Coupon);

    public async Task<Cart?> GetActiveByUserIdAsync(Guid userId, CancellationToken ct = default) =>
        await QueryWithDetails().FirstOrDefaultAsync(c => c.UserId == userId, ct);

    public async Task<Cart?> GetActiveByGuestTokenAsync(string guestToken, CancellationToken ct = default) =>
        await QueryWithDetails().FirstOrDefaultAsync(c => c.GuestToken == guestToken, ct);

    public async Task AddItemAsync(CartItem item, CancellationToken ct = default) =>
        await Context.Set<CartItem>().AddAsync(item, ct);
}

public class CouponRepository(AppDbContext context) : Repository<Coupon>(context), ICouponRepository
{
    public async Task<Coupon?> GetByCodeAsync(string code, CancellationToken ct = default) =>
        await DbSet.FirstOrDefaultAsync(c => c.Code == code, ct);
}

public class OrderRepository(AppDbContext context) : Repository<Order>(context), IOrderRepository
{
    public async Task<Order?> GetByOrderNumberAsync(string orderNumber, CancellationToken ct = default) =>
        await DbSet.Include(o => o.Items).FirstOrDefaultAsync(o => o.OrderNumber == orderNumber, ct);

    public async Task<List<Order>> GetPendingExpiredReservationsAsync(DateTimeOffset asOf, CancellationToken ct = default) =>
        await DbSet.Include(o => o.Items)
            .Where(o => o.Status == OrderStatus.Pending
                        && o.PaymentStatus == PaymentStatus.Pending
                        && o.StockReservationExpiresAt != null
                        && o.StockReservationExpiresAt < asOf)
            .ToListAsync(ct);
}

public class WishlistRepository(AppDbContext context) : Repository<Wishlist>(context), IWishlistRepository
{
    public async Task<Wishlist?> GetByUserIdAsync(Guid userId, CancellationToken ct = default) =>
        await DbSet.Include(w => w.Items).ThenInclude(i => i.Product).FirstOrDefaultAsync(w => w.UserId == userId, ct);

    public async Task AddItemAsync(WishlistItem item, CancellationToken ct = default) =>
        await Context.Set<WishlistItem>().AddAsync(item, ct);
}

public class ReviewRepository(AppDbContext context) : Repository<Review>(context), IReviewRepository
{
    public async Task<bool> HasUserReviewedAsync(Guid productId, Guid userId, CancellationToken ct = default) =>
        await DbSet.AnyAsync(r => r.ProductId == productId && r.UserId == userId, ct);
}
