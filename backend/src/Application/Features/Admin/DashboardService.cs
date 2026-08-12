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
     .GroupBy(i => new
     {
         i.ProductId,
         i.ProductNameSnapshot
     })
     .Select(g => new
     {
         ProductId = g.Key.ProductId,
         ProductName = g.Key.ProductNameSnapshot,
         UnitsSold = g.Sum(i => i.Quantity),
         Revenue = g.Sum(i => i.LineTotal)
     })
     .OrderByDescending(x => x.UnitsSold)
     .Take(5)
     .Select(x => new TopProductDto(
         x.ProductId ?? Guid.Empty,
         x.ProductName,
         x.UnitsSold,
         x.Revenue))
     .ToListAsync(ct);

        return new DashboardStatsDto(totalRevenue, totalOrders, pendingOrders, totalCustomers, totalProducts, lowStockProducts, topProducts);
    }
}
