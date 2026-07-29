using Microsoft.AspNetCore.Mvc;
using MoleculeByMakeover.Application.Features.Catalog;

namespace MoleculeByMakeover.API.Controllers;

public class ProductsController(IProductService productService) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult> Search([FromQuery] ProductQueryParameters query, CancellationToken ct)
    {
        var result = await productService.SearchAsync(query, ct);
        return Ok(new { result.Items, result.Page, result.PageSize, result.TotalCount, result.TotalPages });
    }

    [HttpGet("{slug}")]
    public async Task<ActionResult<ProductDetailDto>> GetBySlug(string slug, CancellationToken ct) =>
        Ok(await productService.GetBySlugAsync(slug, ct));

    [HttpGet("{id:guid}/related")]
    public async Task<ActionResult<List<ProductListItemDto>>> GetRelated(Guid id, CancellationToken ct) =>
        Ok(await productService.GetRelatedAsync(id, ct: ct));
}
