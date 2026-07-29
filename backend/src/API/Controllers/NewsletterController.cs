using Microsoft.AspNetCore.Mvc;
using MoleculeByMakeover.Application.Features.Marketing;

namespace MoleculeByMakeover.API.Controllers;

public class NewsletterController(INewsletterService newsletterService) : ApiControllerBase
{
    [HttpPost("subscribe")]
    public async Task<IActionResult> Subscribe(NewsletterSubscribeRequest request, CancellationToken ct)
    {
        await newsletterService.SubscribeAsync(request, ct);
        return NoContent();
    }
}
