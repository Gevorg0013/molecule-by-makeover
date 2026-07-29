using Microsoft.AspNetCore.Mvc;
using MoleculeByMakeover.Application.Features.Content;

namespace MoleculeByMakeover.API.Controllers;

public class PagesController(IPageService pageService) : ApiControllerBase
{
    [HttpGet("{key}")]
    public async Task<ActionResult<PageDto>> GetByKey(string key, CancellationToken ct) =>
        Ok(await pageService.GetByKeyAsync(key, ct));
}
