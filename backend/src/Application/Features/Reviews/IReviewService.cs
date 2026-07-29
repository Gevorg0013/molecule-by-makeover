namespace MoleculeByMakeover.Application.Features.Reviews;

public interface IReviewService
{
    Task<List<ReviewDto>> GetApprovedForProductAsync(Guid productId, CancellationToken ct = default);
    Task<Guid> CreateAsync(Guid productId, Guid userId, CreateReviewRequest request, CancellationToken ct = default);

    Task<List<ReviewDto>> GetAllAdminAsync(bool? approvedOnly, CancellationToken ct = default);
    Task ApproveAsync(Guid reviewId, CancellationToken ct = default);
    Task DeleteAsync(Guid reviewId, CancellationToken ct = default);
}
