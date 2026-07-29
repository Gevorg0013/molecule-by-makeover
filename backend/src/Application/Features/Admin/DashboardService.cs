using Microsoft.EntityFrameworkCore;
using MoleculeByMakeover.Domain.Enums;
using MoleculeByMakeover.Domain.Interfaces;

namespace MoleculeByMakeover.Application.Features.Admin;

public class DashboardService(IUnitOfWork unitOfWork) : IDashboardService
{
    public async Task<DashboardStatsDto> GetStatsAsync(CancellationToken ct = default)
    {
        var paidOrders = unitOfWork.Orders.Query().Where(o => o.PaymentStatus == PaymentStatus.Paid);

        var totalRevenue = await paidOrders.SumAsync(o => (decimal?)o.GrandTotal, ct) ?? 0m;
        var totalOrders = await unitOfWork.Orders.Query().CountAsync(ct);
        var pendingOrders = await unitOfWork.Orders.Query().CountAsync(o => o.Status == OrderStatus.Pending, ct);
        var totalCustomers = await unitOfWork.Users.Query().CountAsync(ct);
        var totalProducts = await unitOfWork.Products.Query().CountAsync(p => !p.IsDeleted, ct);
        var lowStockProducts = await unitOfWork.Products.Query().CountAsync(p => !p.IsDeleted && p.Stock <= 5, ct);

        var topProducts = await unitOfWork.Orders.Query()
            .Where(o => o.PaymentStatus == PaymentStatus.Paid)
            .SelectMany(o => o.Items)
            .GroupBy(i => new { i.ProductId, i.ProductNameSnapshot })
            .Select(g => new TopProductDto(g.Key.ProductId ?? Guid.Empty, g.Key.ProductNameSnapshot, g.Sum(i => i.Quantity), g.Sum(i => i.LineTotal)))
            .OrderByDescending(p => p.UnitsSold)
            .Take(5)
            .ToListAsync(ct);

        return new DashboardStatsDto(totalRevenue, totalOrders, pendingOrders, totalCustomers, totalProducts, lowStockProducts, topProducts);
    }
}
