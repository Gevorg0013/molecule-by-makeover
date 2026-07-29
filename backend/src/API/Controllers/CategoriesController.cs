using Microsoft.AspNetCore.Mvc;
using MoleculeByMakeover.Application.Features.Catalog;

namespace MoleculeByMakeover.API.Controllers;

public class CategoriesController(ICategoryService categoryService) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<CategoryDto>>> GetTree(CancellationToken ct) =>
        Ok(await categoryService.GetTreeAsync(ct));

    [HttpGet("{slug}")]
    public async Task<ActionResult<CategoryDto>> GetBySlug(string slug, CancellationToken ct) =>
        Ok(await categoryService.GetBySlugAsync(slug, ct));
}
