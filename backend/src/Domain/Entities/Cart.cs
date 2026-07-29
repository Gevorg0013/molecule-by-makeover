using MoleculeByMakeover.Domain.Common;

namespace MoleculeByMakeover.Domain.Entities;

public class Cart : BaseEntity
{
    public Guid? UserId { get; set; }
    public User? User { get; set; }

    public string? GuestToken { get; set; }

    public Guid? CouponId { get; set; }
    public Coupon? Coupon { get; set; }

    public DateTimeOffset ExpiresAt { get; set; } = DateTimeOffset.UtcNow.AddDays(30);

    public ICollection<CartItem> Items { get; set; } = [];
}

public class CartItem : BaseEntity
{
    public Guid CartId { get; set; }
    public Cart Cart { get; set; } = default!;

    public Guid ProductId { get; set; }
    public Product Product { get; set; } = default!;

    public int Quantity { get; set; }
    public decimal UnitPriceSnapshot { get; set; }
}
