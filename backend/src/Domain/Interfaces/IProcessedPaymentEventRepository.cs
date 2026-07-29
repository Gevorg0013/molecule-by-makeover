using MoleculeByMakeover.Domain.Entities;

namespace MoleculeByMakeover.Domain.Interfaces;

public interface IProcessedPaymentEventRepository : IRepository<ProcessedPaymentEvent>
{
    Task<bool> HasBeenProcessedAsync(string providerKey, string eventId, CancellationToken ct = default);
}
