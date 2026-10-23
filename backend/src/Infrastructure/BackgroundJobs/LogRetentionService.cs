using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MoleculeByMakeover.Infrastructure.Persistence;

namespace MoleculeByMakeover.Infrastructure.BackgroundJobs;

// Deletes database log rows past their retention window once a day, so the AppLogs,
// AuthenticationLogs and DataChangeLogs tables (which hold IPs, emails and customer data) don't
// keep personal data indefinitely.
public class LogRetentionService(
    IServiceScopeFactory scopeFactory,
    IConfiguration configuration,
    ILogger<LogRetentionService> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromDays(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var appLogDays = configuration.GetValue("DatabaseLogging:AppLogRetentionDays", 30);
        var authLogDays = configuration.GetValue("DatabaseLogging:AuthenticationLogRetentionDays", 90);
        var dataChangeDays = configuration.GetValue("DatabaseLogging:DataChangeLogRetentionDays", 365);

        using var timer = new PeriodicTimer(Interval);
        do
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var appCutoff = DateTimeOffset.UtcNow.AddDays(-appLogDays);
                var authCutoff = DateTimeOffset.UtcNow.AddDays(-authLogDays);
                var appDeleted = await db.AppLogs.Where(l => l.Timestamp < appCutoff).ExecuteDeleteAsync(stoppingToken);
                var authDeleted = await db.AuthenticationLogs.Where(l => l.OccurredAt < authCutoff).ExecuteDeleteAsync(stoppingToken);
                var dataChangeCutoff = DateTimeOffset.UtcNow.AddDays(-dataChangeDays);
                var dataChangeDeleted = await db.DataChangeLogs.Where(l => l.ChangedAt < dataChangeCutoff).ExecuteDeleteAsync(stoppingToken);

                if (appDeleted + authDeleted + dataChangeDeleted > 0)
                    logger.LogInformation(
                        "Log retention removed {AppLogCount} app log(s), {AuthLogCount} authentication log(s) and {DataChangeCount} data change log(s).",
                        appDeleted, authDeleted, dataChangeDeleted);
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                logger.LogError(ex, "Failed to apply log retention.");
            }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
