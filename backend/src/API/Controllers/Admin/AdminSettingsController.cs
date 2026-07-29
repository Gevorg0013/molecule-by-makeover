using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoleculeByMakeover.Application.Features.Admin;
using MoleculeByMakeover.Shared.Constants;

namespace MoleculeByMakeover.API.Controllers.Admin;

[Route("api/v1/admin/settings")]
[Authorize(Roles = RoleNames.Admin)]
public class AdminSettingsController(ISettingsService settingsService) : ApiControllerBase
{
    [HttpGet("{group}")]
    public async Task<ActionResult<List<SettingDto>>> GetByGroup(string group, CancellationToken ct) =>
        Ok(await settingsService.GetByGroupAsync(group, ct));

    [HttpPut("{group}/{key}")]
    public async Task<IActionResult> Set(string group, string key, UpdateSettingRequest request, CancellationToken ct)
    {
        await settingsService.SetAsync(key, group, request, ct);
        return NoContent();
    }
}

[Route("api/v1/admin/languages")]
[Authorize(Roles = RoleNames.Admin)]
public class AdminLanguagesController(ILanguageService languageService) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<LanguageDto>>> GetAll(CancellationToken ct) => Ok(await languageService.GetAllAsync(ct));

    [HttpPost]
    public async Task<IActionResult> Create(LanguageUpsertRequest request, CancellationToken ct)
    {
        var id = await languageService.CreateAsync(request, ct);
        return Ok(new { id });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, LanguageUpsertRequest request, CancellationToken ct)
    {
        await languageService.UpdateAsync(id, request, ct);
        return NoContent();
    }
}
