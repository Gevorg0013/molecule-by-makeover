using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoleculeByMakeover.Application.Features.Content;
using MoleculeByMakeover.Shared.Constants;

namespace MoleculeByMakeover.API.Controllers.Admin;

[Route("api/v1/admin/blog")]
[Authorize(Roles = RoleNames.Admin)]
public class AdminBlogController(IBlogService blogService) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<BlogPostAdminDto>>> GetAll(CancellationToken ct) => Ok(await blogService.GetAllAdminAsync(ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BlogPostAdminDto>> GetById(Guid id, CancellationToken ct) => Ok(await blogService.GetByIdAdminAsync(id, ct));

    [HttpPost]
    public async Task<ActionResult> Create(BlogPostUpsertRequest request, CancellationToken ct)
    {
        var id = await blogService.CreateAsync(CurrentUserId, request, ct);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, BlogPostUpsertRequest request, CancellationToken ct)
    {
        await blogService.UpdateAsync(id, request, ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await blogService.DeleteAsync(id, ct);
        return NoContent();
    }
}

[Route("api/v1/admin/pages")]
[Authorize(Roles = RoleNames.Admin)]
public class AdminPagesController(IPageService pageService) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<PageAdminDto>>> GetAll(CancellationToken ct) => Ok(await pageService.GetAllAdminAsync(ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PageAdminDto>> GetById(Guid id, CancellationToken ct) => Ok(await pageService.GetByIdAdminAsync(id, ct));

    [HttpPost]
    public async Task<ActionResult> Create(PageUpsertRequest request, CancellationToken ct)
    {
        var id = await pageService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, PageUpsertRequest request, CancellationToken ct)
    {
        await pageService.UpdateAsync(id, request, ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await pageService.DeleteAsync(id, ct);
        return NoContent();
    }
}
