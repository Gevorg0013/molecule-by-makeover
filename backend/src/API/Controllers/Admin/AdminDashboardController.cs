using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoleculeByMakeover.Application.Features.Admin;
using MoleculeByMakeover.Shared.Constants;

namespace MoleculeByMakeover.API.Controllers.Admin;

[Route("api/v1/admin/dashboard")]
[Authorize(Roles = RoleNames.Admin)]
public class AdminDashboardController(IDashboardService dashboardService) : ApiControllerBase
{
    [HttpGet("stats")]
    public async Task<ActionResult<DashboardStatsDto>> GetStats(CancellationToken ct) => Ok(await dashboardService.GetStatsAsync(ct));
}
