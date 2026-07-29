namespace MoleculeByMakeover.Application.Features.Ordering;

public record CartContext(Guid? UserId, string? GuestToken);

public record CartItemDto(Guid Id, Guid ProductId, string ProductName, string Slug, string? ImageUrl, decimal UnitPrice, int Quantity, decimal LineTotal, int AvailableStock);

public record CartDto(Guid Id, List<CartItemDto> Items, decimal SubTotal, decimal DiscountTotal, decimal GrandTotal, string? CouponCode, string Currency);

public record AddCartItemRequest(Guid ProductId, int Quantity);

public record UpdateCartItemRequest(int Quantity);

public record ApplyCouponRequest(string Code);
