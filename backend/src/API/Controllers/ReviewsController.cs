using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoleculeByMakeover.Application.Features.Reviews;

namespace MoleculeByMakeover.API.Controllers;

[Route("api/v1/products/{productId:guid}/reviews")]
public class ReviewsController(IReviewService reviewService) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<ReviewDto>>> GetForProduct(Guid productId, CancellationToken ct) =>
        Ok(await reviewService.GetApprovedForProductAsync(productId, ct));

    [HttpPost]
    [Authorize]
    public async Task<ActionResult> Create(Guid productId, CreateReviewRequest request, CancellationToken ct)
    {
        var id = await reviewService.CreateAsync(productId, CurrentUserId, request, ct);
        return CreatedAtAction(nameof(GetForProduct), new { productId }, new { id });
    }
}
