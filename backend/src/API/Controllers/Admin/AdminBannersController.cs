using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoleculeByMakeover.Application.Features.Marketing;
using MoleculeByMakeover.Application.Features.Media;
using MoleculeByMakeover.Shared.Constants;
using MoleculeByMakeover.Shared.Exceptions;

namespace MoleculeByMakeover.API.Controllers.Admin;

[Route("api/v1/admin/banners")]
[Authorize(Roles = RoleNames.Admin)]
public class AdminBannersController(IBannerService bannerService, IMediaService mediaService) : ApiControllerBase
{
    private const int MaxUploadBytes = 10 * 1024 * 1024;

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

    // Not scoped to a banner id: the admin form uploads the image before the banner exists on
    // create, then persists the returned URL as part of the upsert payload.
    [HttpPost("image")]
    [RequestSizeLimit(MaxUploadBytes)]
    public async Task<ActionResult<GalleryImageDto>> UploadImage(IFormFile file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            throw new BadRequestException("No file was uploaded.");

        await using var stream = file.OpenReadStream();
        var uploaded = await mediaService.UploadAsync(stream, file.FileName, file.ContentType, CurrentUser.UserId, null, MediaFolders.Banners, ct);
        return Ok(uploaded);
    }
}
