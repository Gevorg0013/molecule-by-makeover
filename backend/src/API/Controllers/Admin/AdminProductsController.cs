using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoleculeByMakeover.Application.Features.Catalog;
using MoleculeByMakeover.Application.Features.Media;
using MoleculeByMakeover.Shared.Constants;
using MoleculeByMakeover.Shared.Exceptions;

namespace MoleculeByMakeover.API.Controllers.Admin;

[Route("api/v1/admin/products")]
[Authorize(Roles = RoleNames.Admin)]
public class AdminProductsController(IProductService productService, IMediaService mediaService) : ApiControllerBase
{
    private const int MaxUploadBytes = 10 * 1024 * 1024;

    [HttpGet]
    public async Task<ActionResult<List<ProductAdminDto>>> GetAll(CancellationToken ct) =>
        Ok(await productService.GetAllAdminAsync(ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProductAdminDto>> GetById(Guid id, CancellationToken ct) =>
        Ok(await productService.GetByIdAdminAsync(id, ct));

    [HttpPost]
    public async Task<ActionResult> Create(ProductUpsertRequest request, CancellationToken ct)
    {
        var id = await productService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, ProductUpsertRequest request, CancellationToken ct)
    {
        await productService.UpdateAsync(id, request, ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await productService.DeleteAsync(id, ct);
        return NoContent();
    }

    // Not scoped to a product id: the admin form uploads the main image before the product exists
    // on create, then persists the returned URL as part of the upsert payload.
    [HttpPost("main-image")]
    [RequestSizeLimit(MaxUploadBytes)]
    public async Task<ActionResult<GalleryImageDto>> UploadMainImage(IFormFile file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            throw new BadRequestException("No file was uploaded.");

        await using var stream = file.OpenReadStream();
        var uploaded = await mediaService.UploadAsync(stream, file.FileName, file.ContentType, CurrentUser.UserId, null, MediaFolders.Products, ct);
        return Ok(uploaded);
    }

    [HttpPost("{id:guid}/images")]
    [RequestSizeLimit(MaxUploadBytes)]
    public async Task<ActionResult<ProductImageDto>> UploadImage(Guid id, IFormFile file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            throw new BadRequestException("No file was uploaded.");

        await using var stream = file.OpenReadStream();
        var uploaded = await mediaService.UploadAsync(stream, file.FileName, file.ContentType, CurrentUser.UserId, null, MediaFolders.Products, ct);
        var image = await productService.AddImageAsync(id, uploaded.Url, file.FileName, ct);
        return Ok(image);
    }

    [HttpDelete("{id:guid}/images/{imageId:guid}")]
    public async Task<IActionResult> RemoveImage(Guid id, Guid imageId, CancellationToken ct)
    {
        await productService.RemoveImageAsync(id, imageId, ct);
        return NoContent();
    }
}
