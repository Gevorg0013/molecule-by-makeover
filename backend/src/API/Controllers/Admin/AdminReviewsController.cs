using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoleculeByMakeover.Application.Features.Reviews;
using MoleculeByMakeover.Shared.Constants;

namespace MoleculeByMakeover.API.Controllers.Admin;

[Route("api/v1/admin/reviews")]
[Authorize(Roles = RoleNames.Admin)]
public class AdminReviewsController(IReviewService reviewService) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<ReviewDto>>> GetAll([FromQuery] bool? approvedOnly, CancellationToken ct) =>
        Ok(await reviewService.GetAllAdminAsync(approvedOnly, ct));

    [HttpPut("{id:guid}/approve")]
    public async Task<IActionResult> Approve(Guid id, CancellationToken ct)
    {
        await reviewService.ApproveAsync(id, ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await reviewService.DeleteAsync(id, ct);
        return NoContent();
    }
}
