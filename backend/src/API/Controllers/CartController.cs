using Microsoft.AspNetCore.Mvc;
using MoleculeByMakeover.API.Common;
using MoleculeByMakeover.Application.Features.Ordering;

namespace MoleculeByMakeover.API.Controllers;

public class CartController(ICartService cartService) : ApiControllerBase
{
    private CartContext ResolveContext() =>
        CurrentUser.UserId is { } userId
            ? new CartContext(userId, null)
            : new CartContext(null, HttpContext.GetOrCreateGuestToken());

    [HttpGet]
    public async Task<ActionResult<CartDto>> Get(CancellationToken ct) =>
        Ok(await cartService.GetCartAsync(ResolveContext(), ct));

    [HttpPost("items")]
    public async Task<ActionResult<CartDto>> AddItem(AddCartItemRequest request, CancellationToken ct) =>
        Ok(await cartService.AddItemAsync(ResolveContext(), request, ct));

    [HttpPatch("items/{itemId:guid}")]
    public async Task<ActionResult<CartDto>> UpdateItem(Guid itemId, UpdateCartItemRequest request, CancellationToken ct) =>
        Ok(await cartService.UpdateItemAsync(ResolveContext(), itemId, request, ct));

    [HttpDelete("items/{itemId:guid}")]
    public async Task<ActionResult<CartDto>> RemoveItem(Guid itemId, CancellationToken ct) =>
        Ok(await cartService.RemoveItemAsync(ResolveContext(), itemId, ct));

    [HttpPost("coupon")]
    public async Task<ActionResult<CartDto>> ApplyCoupon(ApplyCouponRequest request, CancellationToken ct) =>
        Ok(await cartService.ApplyCouponAsync(ResolveContext(), request, ct));

    [HttpDelete("coupon")]
    public async Task<ActionResult<CartDto>> RemoveCoupon(CancellationToken ct) =>
        Ok(await cartService.RemoveCouponAsync(ResolveContext(), ct));
}
