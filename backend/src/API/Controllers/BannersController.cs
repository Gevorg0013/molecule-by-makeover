using Microsoft.AspNetCore.Mvc;
using MoleculeByMakeover.Application.Features.Marketing;

namespace MoleculeByMakeover.API.Controllers;

public class BannersController(IBannerService bannerService) : ApiControllerBase
{
    [HttpGet("active")]
    public async Task<ActionResult<List<BannerDto>>> GetActive(CancellationToken ct) =>
        Ok(await bannerService.GetActiveAsync(ct));
}
