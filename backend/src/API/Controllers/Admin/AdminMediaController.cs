using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoleculeByMakeover.Application.Features.Media;
using MoleculeByMakeover.Shared.Constants;

namespace MoleculeByMakeover.API.Controllers.Admin;

[Route("api/v1/admin/media")]
[Authorize(Roles = RoleNames.Admin)]
public class AdminMediaController(IMediaService mediaService) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<GalleryImageDto>>> GetAll(CancellationToken ct) => Ok(await mediaService.GetAllAsync(ct));

    [HttpPost]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<ActionResult<GalleryImageDto>> Upload(IFormFile file, [FromForm] string? altText, CancellationToken ct)
    {
        await using var stream = file.OpenReadStream();
        var image = await mediaService.UploadAsync(stream, file.FileName, file.ContentType, CurrentUser.UserId, altText, ct);
        return Ok(image);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await mediaService.DeleteAsync(id, ct);
        return NoContent();
    }
}
