using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MoleculeByMakeover.Application.Common.Interfaces;
using MoleculeByMakeover.Domain.Entities;
using MoleculeByMakeover.Domain.Enums;
using MoleculeByMakeover.Domain.Interfaces;
using MoleculeByMakeover.Shared.Common;
using MoleculeByMakeover.Shared.Exceptions;

namespace MoleculeByMakeover.Application.Features.Ordering;

public class OrderService(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser,
    ILogger<OrderService> logger) : IOrderService
{
    public async Task<PaginatedList<OrderSummaryDto>> GetMyOrdersAsync(Guid userId, int page, int pageSize, CancellationToken ct = default)
    {
        page = Math.Max(page, 1);
        pageSize = pageSize <= 0 ? 20 : Math.Min(pageSize, 100);
        var query = unitOfWork.Orders.Query().Where(o => o.UserId == userId).OrderByDescending(o => o.PlacedAt);
        var total = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);

        return PaginatedList<OrderSummaryDto>.Create(items.Select(ToSummary).ToList(), total, page, pageSize);
    }

    public async Task<OrderDetailDto> GetByOrderNumberAsync(string orderNumber, Guid userId, bool isAdmin, CancellationToken ct = default)
    {
        var order = await unitOfWork.Orders.GetByOrderNumberAsync(orderNumber, ct)
            ?? throw new NotFoundException(nameof(Order), orderNumber);

        if (!isAdmin && order.UserId != userId)
            throw new ForbiddenAccessException("You do not have access to this order.");

        return ToDetail(order);
    }

    public async Task<PaginatedList<OrderSummaryDto>> GetAllAdminAsync(OrderQueryParameters query, CancellationToken ct = default)
    {
        var q = unitOfWork.Orders.Query().AsQueryable();
        if (query.Status is not null) q = q.Where(o => o.Status == query.Status);
        q = q.OrderByDescending(o => o.PlacedAt);

        var total = await q.CountAsync(ct);
        var items = await q.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(ct);

        return PaginatedList<OrderSummaryDto>.Create(items.Select(ToSummary).ToList(), total, query.Page, query.PageSize);
    }

    public async Task UpdateStatusAsync(Guid orderId, UpdateOrderStatusRequest request, CancellationToken ct = default)
    {
        if (!Enum.IsDefined(request.Status))
            throw new BadRequestException("Unknown order status.");

        var order = await unitOfWork.Orders.GetByIdAsync(orderId, ct) ?? throw new NotFoundException(nameof(Order), orderId);

        var previousStatus = order.Status;
        order.Status = request.Status;
        order.UpdatedAt = DateTimeOffset.UtcNow;
        unitOfWork.Orders.Update(order);

        var sale = await unitOfWork.SalesTransactions.GetByOrderIdAsync(order.Id, ct);
        if (sale is not null)
        {
            AppendEvent(sale, SalesTransactionEventType.StatusChanged, SalesEventSource.Admin,
                $"Order status changed {previousStatus} -> {order.Status}",
                new { PreviousStatus = previousStatus, NewStatus = order.Status, order.PaymentStatus }, actorUserId: currentUser.UserId);
            if (order.Status == OrderStatus.Cancelled) sale.Complete(SalesTransactionStatus.Cancelled);
            else if (order.Status is OrderStatus.Delivered or OrderStatus.Completed) sale.Complete(SalesTransactionStatus.Completed);
        }

        await unitOfWork.SaveChangesAsync(ct);

        logger.LogInformation("Order {OrderNumber} status changed {PreviousStatus} -> {NewStatus}",
            order.OrderNumber, previousStatus, order.Status);
    }

    public async Task<int> ReleaseExpiredReservationsAsync(CancellationToken ct = default)
    {
        var expiredOrders = await unitOfWork.Orders.GetPendingExpiredReservationsAsync(DateTimeOffset.UtcNow, ct);

        foreach (var order in expiredOrders)
        {
            order.Status = OrderStatus.Cancelled;
            order.UpdatedAt = DateTimeOffset.UtcNow;
            unitOfWork.Orders.Update(order);

            var released = new List<object>();
            foreach (var item in order.Items.Where(i => i.ProductId is not null))
            {
                var product = await unitOfWork.Products.GetByIdAsync(item.ProductId!.Value, ct);
                if (product is null) continue;
                var stockBefore = product.Stock;
                product.Stock += item.Quantity;
                unitOfWork.Products.Update(product);
                released.Add(new { item.ProductId, product.Sku, item.Quantity, StockBefore = stockBefore, StockAfter = product.Stock });
            }

            var sale = await unitOfWork.SalesTransactions.GetByOrderIdAsync(order.Id, ct);
            if (sale is not null)
            {
                AppendEvent(sale, SalesTransactionEventType.ReservationExpired, SalesEventSource.System,
                    "Stock reservation expired without payment; order cancelled",
                    new { order.StockReservationExpiresAt }, succeeded: false);
                AppendEvent(sale, SalesTransactionEventType.StockReleased, SalesEventSource.System,
                    $"{released.Count} line(s) returned to stock", new { Items = released });
                sale.Complete(SalesTransactionStatus.Cancelled);
            }

            logger.LogInformation("Order {OrderNumber} cancelled: stock reservation expired unpaid, {ItemCount} line(s) restocked",
                order.OrderNumber, order.Items.Count);
        }

        if (expiredOrders.Count > 0)
            await unitOfWork.SaveChangesAsync(ct);

        return expiredOrders.Count;
    }

    public async Task MarkPaidAsync(string providerReference, string? providerEventId = null, CancellationToken ct = default)
    {
        var order = await unitOfWork.Orders.Query().Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.PaymentReference == providerReference, ct);

        if (order is null)
        {
            logger.LogWarning("Payment succeeded for {PaymentReference} but no matching order was found", providerReference);
            return;
        }
        if (order.PaymentStatus == PaymentStatus.Paid)
        {
            logger.LogInformation("Order {OrderNumber} already marked paid; ignoring duplicate payment confirmation", order.OrderNumber);
            return;
        }

        order.PaymentStatus = PaymentStatus.Paid;
        order.Status = OrderStatus.Processing;
        order.StockReservationExpiresAt = null;
        order.UpdatedAt = DateTimeOffset.UtcNow;
        unitOfWork.Orders.Update(order);

        var sale = await unitOfWork.SalesTransactions.GetByOrderIdAsync(order.Id, ct);
        if (sale is not null)
        {
            AppendEvent(sale, SalesTransactionEventType.PaymentSucceeded, SalesEventSource.PaymentProvider,
                $"Payment of {order.GrandTotal} {order.Currency} confirmed",
                new { order.GrandTotal, order.Currency, order.PaymentProvider, PaymentReference = providerReference, ProviderEventId = providerEventId });
            sale.Complete(SalesTransactionStatus.Paid);
        }

        await unitOfWork.SaveChangesAsync(ct);

        logger.LogInformation("Order {OrderNumber} paid: {GrandTotal} {Currency} via {Provider} ({PaymentReference})",
            order.OrderNumber, order.GrandTotal, order.Currency, order.PaymentProvider, providerReference);
    }

    public async Task RecordPaymentEventAsync(string providerReference, PaymentEventType eventType, string providerEventId, CancellationToken ct = default)
    {
        var sale = await unitOfWork.SalesTransactions.GetByPaymentReferenceAsync(providerReference, ct);
        if (sale is null)
        {
            logger.LogWarning("{EventType} payment event {EventId} for {PaymentReference} matches no sales transaction",
                eventType, providerEventId, providerReference);
            return;
        }

        var data = new { PaymentReference = providerReference, ProviderEventId = providerEventId };
        switch (eventType)
        {
            case PaymentEventType.PaymentFailed:
                AppendEvent(sale, SalesTransactionEventType.PaymentFailed, SalesEventSource.PaymentProvider, "Payment failed", data, succeeded: false);
                break;
            case PaymentEventType.Refunded:
                AppendEvent(sale, SalesTransactionEventType.Refunded, SalesEventSource.PaymentProvider, "Payment refunded", data);
                sale.Complete(SalesTransactionStatus.Refunded);
                break;
            default:
                AppendEvent(sale, SalesTransactionEventType.PaymentEventReceived, SalesEventSource.PaymentProvider,
                    $"Payment event received ({eventType})", data);
                break;
        }

        await unitOfWork.SaveChangesAsync(ct);
        logger.LogInformation("Recorded {EventType} payment event {EventId} on sales transaction {SalesTransactionId}",
            eventType, providerEventId, sale.Id);
    }

    // The sale was loaded from the database, so the new event has to be tracked explicitly (see TrackNew).
    private void AppendEvent(SalesTransaction sale, SalesTransactionEventType eventType, SalesEventSource source,
        string message, object? data = null, bool succeeded = true, Guid? actorUserId = null) =>
        unitOfWork.TrackNew(sale.Record(eventType, source, message, data, succeeded, actorUserId));

    private static OrderSummaryDto ToSummary(Order order) =>
        new(order.Id, order.OrderNumber, order.Status, order.PaymentStatus, order.GrandTotal, order.Currency, order.PlacedAt);

    private static OrderDetailDto ToDetail(Order order) => new(
        order.Id, order.OrderNumber, order.Status, order.PaymentStatus, order.SubTotal, order.DiscountTotal,
        order.ShippingTotal, order.GrandTotal, order.Currency, order.ShippingAddress, order.PaymentProvider, order.PlacedAt,
        order.Items.Select(i => new OrderItemDto(i.Id, i.ProductId, i.ProductNameSnapshot, i.SkuSnapshot, i.UnitPriceSnapshot, i.Quantity, i.LineTotal)).ToList());
}
