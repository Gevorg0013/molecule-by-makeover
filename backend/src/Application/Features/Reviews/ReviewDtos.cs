namespace MoleculeByMakeover.Application.Features.Reviews;

public record ReviewDto(Guid Id, Guid ProductId, Guid UserId, string ReviewerName, int Rating, string? Comment, bool IsApproved, DateTimeOffset CreatedAt);

public record CreateReviewRequest(int Rating, string? Comment);
