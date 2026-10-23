using MoleculeByMakeover.Domain.Entities;

namespace MoleculeByMakeover.Application.Common.Interfaces;

public interface ISalesTransactionWriter
{
    /// <summary>
    /// Saves a sale attempt that failed, outside the request's unit of work - whose pending changes
    /// (coupon use, stock) must NOT be saved with it. Never throws, so the original error reaches
    /// the customer unchanged.
    /// </summary>
    Task WriteFailedAttemptAsync(SalesTransaction transaction, CancellationToken ct = default);
}
