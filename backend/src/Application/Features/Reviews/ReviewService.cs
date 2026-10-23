using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MoleculeByMakeover.Domain.Entities;
using MoleculeByMakeover.Domain.Interfaces;
using MoleculeByMakeover.Shared.Exceptions;

namespace MoleculeByMakeover.Application.Features.Reviews;

public class ReviewService(IUnitOfWork unitOfWork, IValidator<CreateReviewRequest> validator, ILogger<ReviewService> logger) : IReviewService
{
    public async Task<List<ReviewDto>> GetApprovedForProductAsync(Guid productId, CancellationToken ct = default)
    {
        var reviews = await unitOfWork.Reviews.Query()
            .Include(r => r.User)
            .Where(r => r.ProductId == productId && r.IsApproved)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(ct);

        return reviews.Select(ToDto).ToList();
    }

    public async Task<Guid> CreateAsync(Guid productId, Guid userId, CreateReviewRequest request, CancellationToken ct = default)
    {
        await validator.ValidateAndThrowAsync(request, ct);

        _ = await unitOfWork.Products.GetByIdAsync(productId, ct) ?? throw new NotFoundException(nameof(Product), productId);

        if (await unitOfWork.Reviews.HasUserReviewedAsync(productId, userId, ct))
            throw new ConflictException("You have already reviewed this product.");

        var review = new Review
        {
            ProductId = productId,
            UserId = userId,
            Rating = request.Rating,
            Comment = request.Comment,
            IsApproved = false,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await unitOfWork.Reviews.AddAsync(review, ct);
        await unitOfWork.SaveChangesAsync(ct);
        logger.LogInformation("Review {ReviewId} submitted for product {ProductId} by user {UserId}, rating {Rating}",
            review.Id, productId, userId, review.Rating);
        return review.Id;
    }

    public async Task<List<ReviewDto>> GetAllAdminAsync(bool? approvedOnly, CancellationToken ct = default)
    {
        var query = unitOfWork.Reviews.Query().Include(r => r.User).AsQueryable();
        if (approvedOnly is not null) query = query.Where(r => r.IsApproved == approvedOnly);

        var reviews = await query.OrderByDescending(r => r.CreatedAt).ToListAsync(ct);
        return reviews.Select(ToDto).ToList();
    }

    public async Task ApproveAsync(Guid reviewId, CancellationToken ct = default)
    {
        var review = await unitOfWork.Reviews.GetByIdAsync(reviewId, ct) ?? throw new NotFoundException(nameof(Review), reviewId);
        review.IsApproved = true;
        unitOfWork.Reviews.Update(review);
        await unitOfWork.SaveChangesAsync(ct);
        logger.LogInformation("Review {ReviewId} approved", review.Id);
    }

    public async Task DeleteAsync(Guid reviewId, CancellationToken ct = default)
    {
        var review = await unitOfWork.Reviews.GetByIdAsync(reviewId, ct) ?? throw new NotFoundException(nameof(Review), reviewId);
        unitOfWork.Reviews.Remove(review);
        await unitOfWork.SaveChangesAsync(ct);
        logger.LogInformation("Review {ReviewId} deleted", review.Id);
    }

    private static ReviewDto ToDto(Review r) => new(r.Id, r.ProductId, r.UserId, r.User.FullName, r.Rating, r.Comment, r.IsApproved, r.CreatedAt);
}
