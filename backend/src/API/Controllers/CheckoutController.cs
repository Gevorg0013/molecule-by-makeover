using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoleculeByMakeover.Application.Features.Ordering;

namespace MoleculeByMakeover.API.Controllers;

[Authorize]
public class CheckoutController(ICheckoutService checkoutService, IConfiguration configuration) : ApiControllerBase
{
    [HttpPost]
    public async Task<ActionResult<CheckoutResultDto>> PlaceOrder(CheckoutRequest request, CancellationToken ct)
    {
        var baseUrl = configuration["Frontend:BaseUrl"]!.TrimEnd('/');
        var result = await checkoutService.PlaceOrderAsync(
            CurrentUserId, null, request, $"{baseUrl}/checkout/success", $"{baseUrl}/checkout/cancel", ct);

        return Ok(result);
    }
}
