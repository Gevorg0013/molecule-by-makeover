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
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public OrderStatus? Status { get; set; }
}
