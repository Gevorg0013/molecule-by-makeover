using MoleculeByMakeover.Domain.Enums;
using MoleculeByMakeover.Domain.ValueObjects;

namespace MoleculeByMakeover.Application.Features.Ordering;

public record CheckoutRequest(Address ShippingAddress, string PaymentProviderKey);

public record CheckoutResultDto(string OrderNumber, string? ClientSecret, string? RedirectUrl, decimal GrandTotal, string Currency);

public record OrderItemDto(Guid Id, Guid? ProductId, string ProductName, string Sku, decimal UnitPrice, int Quantity, decimal LineTotal);

public record OrderSummaryDto(Guid Id, string OrderNumber, OrderStatus Status, PaymentStatus PaymentStatus, decimal GrandTotal, string Currency, DateTimeOffset PlacedAt);

public record OrderDetailDto(
    Guid Id,
    string OrderNumber,
    OrderStatus Status,
    PaymentStatus PaymentStatus,
    decimal SubTotal,
    decimal DiscountTotal,
    decimal ShippingTotal,
    decimal GrandTotal,
    string Currency,
    Address ShippingAddress,
    string PaymentProvider,
    DateTimeOffset PlacedAt,
    List<OrderItemDto> Items);

public record UpdateOrderStatusRequest(OrderStatus Status);

public class OrderQueryParameters
{
    private int _page = 1;
    private int _pageSize = 20;

    public int Page { get => _page; set => _page = Math.Max(value, 1); }
    public int PageSize { get => _pageSize; set => _pageSize = value <= 0 ? 20 : Math.Min(value, 100); }
    public OrderStatus? Status { get; set; }
}
