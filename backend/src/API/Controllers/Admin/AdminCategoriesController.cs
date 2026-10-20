using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoleculeByMakeover.Application.Features.Catalog;
using MoleculeByMakeover.Application.Features.Media;
using MoleculeByMakeover.Shared.Constants;
using MoleculeByMakeover.Shared.Exceptions;

namespace MoleculeByMakeover.API.Controllers.Admin;

[Route("api/v1/admin/categories")]
[Authorize(Roles = RoleNames.Admin)]
public class AdminCategoriesController(ICategoryService categoryService, IMediaService mediaService) : ApiControllerBase
{
    private const int MaxUploadBytes = 10 * 1024 * 1024;

    [HttpGet]
    public async Task<ActionResult<List<CategoryAdminDto>>> GetAll(CancellationToken ct) =>
        Ok(await categoryService.GetAllAdminAsync(ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CategoryAdminDto>> GetById(Guid id, CancellationToken ct) =>
        Ok(await categoryService.GetByIdAdminAsync(id, ct));

    [HttpPost]
    public async Task<ActionResult> Create(CategoryUpsertRequest request, CancellationToken ct)
    {
        var id = await categoryService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, CategoryUpsertRequest request, CancellationToken ct)
    {
        await categoryService.UpdateAsync(id, request, ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await categoryService.DeleteAsync(id, ct);
        return NoContent();
    }

    // Not scoped to an id: the admin form uploads the image before the category exists on
    // create, then persists the returned URL as part of the upsert payload.
    [HttpPost("image")]
    [RequestSizeLimit(MaxUploadBytes)]
    public async Task<ActionResult<GalleryImageDto>> UploadImage(IFormFile file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            throw new BadRequestException("No file was uploaded.");

        await using var stream = file.OpenReadStream();
        var uploaded = await mediaService.UploadAsync(stream, file.FileName, file.ContentType, CurrentUser.UserId, null, MediaFolders.Categories, ct);
        return Ok(uploaded);
    }
}
