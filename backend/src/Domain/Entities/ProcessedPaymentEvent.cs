using MoleculeByMakeover.Domain.Common;

namespace MoleculeByMakeover.Domain.Entities;

// Idempotency ledger: prevents a replayed/duplicate webhook delivery from any
// IPaymentProvider (Stripe, Idram, ArCa, Telcell, ...) from re-applying a payment.
public class ProcessedPaymentEvent : BaseEntity
{
    public string ProviderKey { get; set; } = default!;
    public string EventId { get; set; } = default!;
    public DateTimeOffset ProcessedAt { get; set; } = DateTimeOffset.UtcNow;
}
