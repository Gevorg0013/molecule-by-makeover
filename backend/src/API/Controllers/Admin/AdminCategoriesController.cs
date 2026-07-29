using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoleculeByMakeover.Application.Features.Catalog;
using MoleculeByMakeover.Shared.Constants;

namespace MoleculeByMakeover.API.Controllers.Admin;

[Route("api/v1/admin/categories")]
[Authorize(Roles = RoleNames.Admin)]
public class AdminCategoriesController(ICategoryService categoryService) : ApiControllerBase
{
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
}
