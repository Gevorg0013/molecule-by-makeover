using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoleculeByMakeover.Application.Features.Marketing;
using MoleculeByMakeover.Shared.Constants;

namespace MoleculeByMakeover.API.Controllers.Admin;

[Route("api/v1/admin/banners")]
public class AdminBannersController(IBannerService bannerService) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<BannerAdminDto>>> GetAll(CancellationToken ct) => Ok(await bannerService.GetAllAdminAsync(ct));

    [HttpPost]
    public async Task<ActionResult> Create(BannerUpsertRequest request, CancellationToken ct)
    {
        var id = await bannerService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetAll), new { id }, new { id });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, BannerUpsertRequest request, CancellationToken ct)
    {
        await bannerService.UpdateAsync(id, request, ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await bannerService.DeleteAsync(id, ct);
        return NoContent();
    }
}
