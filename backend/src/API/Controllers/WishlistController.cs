using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoleculeByMakeover.Application.Features.Wishlists;

namespace MoleculeByMakeover.API.Controllers;

[Authorize]
public class WishlistController(IWishlistService wishlistService) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<WishlistDto>> Get(CancellationToken ct) =>
        Ok(await wishlistService.GetAsync(CurrentUserId, ct));

    [HttpPost("{productId:guid}")]
    public async Task<IActionResult> Add(Guid productId, CancellationToken ct)
    {
        await wishlistService.AddAsync(CurrentUserId, productId, ct);
        return NoContent();
    }

    [HttpDelete("{productId:guid}")]
    public async Task<IActionResult> Remove(Guid productId, CancellationToken ct)
    {
        await wishlistService.RemoveAsync(CurrentUserId, productId, ct);
        return NoContent();
    }
}
