using Microsoft.EntityFrameworkCore;
using MoleculeByMakeover.Domain.Entities;
using MoleculeByMakeover.Domain.Enums;
using MoleculeByMakeover.Domain.Interfaces;
using MoleculeByMakeover.Shared.Common;
using MoleculeByMakeover.Shared.Exceptions;

namespace MoleculeByMakeover.Application.Features.Ordering;

public class OrderService(IUnitOfWork unitOfWork) : IOrderService
{
    public async Task<PaginatedList<OrderSummaryDto>> GetMyOrdersAsync(Guid userId, int page, int pageSize, CancellationToken ct = default)
    {
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
        var order = await unitOfWork.Orders.GetByIdAsync(orderId, ct) ?? throw new NotFoundException(nameof(Order), orderId);

        order.Status = request.Status;
        order.UpdatedAt = DateTimeOffset.UtcNow;
        unitOfWork.Orders.Update(order);
        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task<int> ReleaseExpiredReservationsAsync(CancellationToken ct = default)
    {
        var expiredOrders = await unitOfWork.Orders.GetPendingExpiredReservationsAsync(DateTimeOffset.UtcNow, ct);

        foreach (var order in expiredOrders)
        {
            order.Status = OrderStatus.Cancelled;
            order.UpdatedAt = DateTimeOffset.UtcNow;
            unitOfWork.Orders.Update(order);

            foreach (var item in order.Items.Where(i => i.ProductId is not null))
            {
                var product = await unitOfWork.Products.GetByIdAsync(item.ProductId!.Value, ct);
                if (product is null) continue;
                product.Stock += item.Quantity;
                unitOfWork.Products.Update(product);
            }
        }

        if (expiredOrders.Count > 0)
            await unitOfWork.SaveChangesAsync(ct);

        return expiredOrders.Count;
    }

    public async Task MarkPaidAsync(string providerReference, CancellationToken ct = default)
    {
        var order = await unitOfWork.Orders.Query().Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.PaymentReference == providerReference, ct);

        if (order is null || order.PaymentStatus == PaymentStatus.Paid) return;

        order.PaymentStatus = PaymentStatus.Paid;
        order.Status = OrderStatus.Processing;
        order.StockReservationExpiresAt = null;
        order.UpdatedAt = DateTimeOffset.UtcNow;
        unitOfWork.Orders.Update(order);
        await unitOfWork.SaveChangesAsync(ct);
    }

    private static OrderSummaryDto ToSummary(Order order) =>
        new(order.Id, order.OrderNumber, order.Status, order.PaymentStatus, order.GrandTotal, order.Currency, order.PlacedAt);

    private static OrderDetailDto ToDetail(Order order) => new(
        order.Id, order.OrderNumber, order.Status, order.PaymentStatus, order.SubTotal, order.DiscountTotal,
        order.ShippingTotal, order.GrandTotal, order.Currency, order.ShippingAddress, order.PaymentProvider, order.PlacedAt,
        order.Items.Select(i => new OrderItemDto(i.Id, i.ProductId, i.ProductNameSnapshot, i.SkuSnapshot, i.UnitPriceSnapshot, i.Quantity, i.LineTotal)).ToList());
}
