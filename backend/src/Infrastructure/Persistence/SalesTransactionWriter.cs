using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MoleculeByMakeover.Application.Common.Interfaces;
using MoleculeByMakeover.Domain.Entities;

namespace MoleculeByMakeover.Infrastructure.Persistence;

// Uses its own DbContext scope so the failed request's tracked changes are never flushed with it.
public class SalesTransactionWriter(IServiceScopeFactory scopeFactory, ILogger<SalesTransactionWriter> logger) : ISalesTransactionWriter
{
    public async Task WriteFailedAttemptAsync(SalesTransaction transaction, CancellationToken ct = default)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            dbContext.SalesTransactions.Add(transaction);
            // Not tied to the request token: a client hanging up mustn't drop the audit record.
            await dbContext.SaveChangesAsync(CancellationToken.None);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to write sales transaction {SalesTransactionId} for user {UserId} ({Status})",
                transaction.Id, transaction.UserId, transaction.Status);
        }
    }
}
