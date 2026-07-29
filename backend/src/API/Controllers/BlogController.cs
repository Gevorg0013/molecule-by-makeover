using Microsoft.AspNetCore.Mvc;
using MoleculeByMakeover.Application.Features.Content;

namespace MoleculeByMakeover.API.Controllers;

public class BlogController(IBlogService blogService) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult> GetPublished([FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default)
    {
        var result = await blogService.GetPublishedAsync(page, pageSize, ct);
        return Ok(new { result.Items, result.Page, result.PageSize, result.TotalCount, result.TotalPages });
    }

    [HttpGet("{slug}")]
    public async Task<ActionResult<BlogPostDetailDto>> GetBySlug(string slug, CancellationToken ct) =>
        Ok(await blogService.GetBySlugAsync(slug, ct));
}
