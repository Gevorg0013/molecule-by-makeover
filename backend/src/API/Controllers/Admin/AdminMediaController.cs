using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoleculeByMakeover.Application.Features.Media;
using MoleculeByMakeover.Shared.Constants;
using MoleculeByMakeover.Shared.Exceptions;

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
        if (file is null || file.Length == 0)
            throw new BadRequestException("No file was uploaded.");

        await using var stream = file.OpenReadStream();
        var image = await mediaService.UploadAsync(stream, file.FileName, file.ContentType, CurrentUser.UserId, altText, MediaFolders.Gallery, ct);
        return Ok(image);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await mediaService.DeleteAsync(id, ct);
        return NoContent();
    }
}
