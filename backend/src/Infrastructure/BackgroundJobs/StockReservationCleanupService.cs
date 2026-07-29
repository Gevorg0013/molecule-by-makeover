using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MoleculeByMakeover.Application.Features.Ordering;

namespace MoleculeByMakeover.Infrastructure.BackgroundJobs;

// Cancels Pending/unpaid orders whose stock reservation has expired and restocks their
// items - see CheckoutService, which reserves stock immediately at order creation.
public class StockReservationCleanupService(IServiceScopeFactory scopeFactory, ILogger<StockReservationCleanupService> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Interval);
        do
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();
                var released = await orderService.ReleaseExpiredReservationsAsync(stoppingToken);
                if (released > 0)
                    logger.LogInformation("Released stock reservations for {Count} expired order(s).", released);
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                logger.LogError(ex, "Failed to release expired stock reservations.");
            }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
