using MoleculeByMakeover.Shared.Common;

namespace MoleculeByMakeover.Application.Features.Ordering;

public interface IOrderService
{
    Task<PaginatedList<OrderSummaryDto>> GetMyOrdersAsync(Guid userId, int page, int pageSize, CancellationToken ct = default);
    Task<OrderDetailDto> GetByOrderNumberAsync(string orderNumber, Guid userId, bool isAdmin, CancellationToken ct = default);

    Task<PaginatedList<OrderSummaryDto>> GetAllAdminAsync(OrderQueryParameters query, CancellationToken ct = default);
    Task UpdateStatusAsync(Guid orderId, UpdateOrderStatusRequest request, CancellationToken ct = default);

    /// <summary>Cancels stale unpaid orders and releases their reserved stock. Called by a background job.</summary>
    Task<int> ReleaseExpiredReservationsAsync(CancellationToken ct = default);

    /// <summary>Applies a confirmed payment (called from the payment webhook handler).</summary>
    Task MarkPaidAsync(string providerReference, CancellationToken ct = default);
}
