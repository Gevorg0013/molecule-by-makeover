namespace MoleculeByMakeover.Application.Features.Admin;

public interface IDashboardService
{
    Task<DashboardStatsDto> GetStatsAsync(CancellationToken ct = default);
}
