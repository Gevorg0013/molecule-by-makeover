namespace MoleculeByMakeover.Application.Features.Ordering;

public interface ICartService
{
    Task<CartDto> GetCartAsync(CartContext context, CancellationToken ct = default);
    Task<CartDto> AddItemAsync(CartContext context, AddCartItemRequest request, CancellationToken ct = default);
    Task<CartDto> UpdateItemAsync(CartContext context, Guid itemId, UpdateCartItemRequest request, CancellationToken ct = default);
    Task<CartDto> RemoveItemAsync(CartContext context, Guid itemId, CancellationToken ct = default);
    Task<CartDto> ApplyCouponAsync(CartContext context, ApplyCouponRequest request, CancellationToken ct = default);
    Task<CartDto> RemoveCouponAsync(CartContext context, CancellationToken ct = default);

    /// <summary>Merges a guest cart into the now-authenticated user's cart on login.</summary>
    Task MergeGuestCartAsync(string guestToken, Guid userId, CancellationToken ct = default);
}
