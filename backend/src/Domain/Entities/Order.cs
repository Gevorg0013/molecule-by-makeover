using MoleculeByMakeover.Domain.Common;
using MoleculeByMakeover.Domain.Enums;
using MoleculeByMakeover.Domain.ValueObjects;

namespace MoleculeByMakeover.Domain.Entities;

public class Order : BaseEntity
{
    public string OrderNumber { get; set; } = default!;

    public Guid UserId { get; set; }
    public User User { get; set; } = default!;

    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

    public Guid? CouponId { get; set; }
    public Coupon? Coupon { get; set; }

    public decimal SubTotal { get; set; }
    public decimal DiscountTotal { get; set; }
    public decimal ShippingTotal { get; set; }
    public decimal GrandTotal { get; set; }
    public string Currency { get; set; } = "AMD";

    public Address ShippingAddress { get; set; } = default!;

    public string PaymentProvider { get; set; } = default!;
    public string? PaymentReference { get; set; }

    public DateTimeOffset PlacedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? StockReservationExpiresAt { get; set; }

    public ICollection<OrderItem> Items { get; set; } = [];
}

public class OrderItem : BaseEntity
{
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = default!;

    public Guid? ProductId { get; set; }
    public Product? Product { get; set; }

    public string ProductNameSnapshot { get; set; } = default!;
    public string SkuSnapshot { get; set; } = default!;
    public decimal UnitPriceSnapshot { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; }
}
