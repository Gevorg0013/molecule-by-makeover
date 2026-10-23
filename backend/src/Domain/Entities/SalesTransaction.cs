using MoleculeByMakeover.Domain.Enums;

namespace MoleculeByMakeover.Domain.Entities;

// Audit trail of one sale, from the moment checkout starts until it is paid, cancelled or refunded.
// A row exists for every checkout attempt - including rejected ones that never became an Order - and
// each step is appended to Events. Holds amounts and references only: no card data (payments are
// tokenized by the provider) and no shipping address (that stays on the Order).
public class SalesTransaction
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }

    /// <summary>Set once the order is created; null for attempts rejected before that.</summary>
    public Guid? OrderId { get; set; }
    public string? OrderNumber { get; set; }

    public SalesTransactionStatus Status { get; set; } = SalesTransactionStatus.Started;

    public int ItemCount { get; set; }
    public decimal? SubTotal { get; set; }
    public decimal? DiscountTotal { get; set; }
    public decimal? ShippingTotal { get; set; }
    public decimal? GrandTotal { get; set; }
    public string? Currency { get; set; }
    public string? CouponCode { get; set; }

    public string PaymentProvider { get; set; } = default!;
    public string? PaymentReference { get; set; }

    /// <summary>Customer-safe reason for a rejected attempt.</summary>
    public string? FailureReason { get; set; }

    /// <summary>All timestamps are UTC.</summary>
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    /// <summary>When the sale reached a final state (rejected, paid, cancelled, refunded or completed).</summary>
    public DateTimeOffset? CompletedAt { get; set; }

    public ICollection<SalesTransactionEvent> Events { get; set; } = [];
}

public class SalesTransactionEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid SalesTransactionId { get; set; }
    public SalesTransaction SalesTransaction { get; set; } = default!;

    public SalesTransactionEventType EventType { get; set; }
    public SalesEventSource Source { get; set; }
    public bool Succeeded { get; set; } = true;
    public string Message { get; set; } = default!;

    /// <summary>Step details (items, amounts, statuses) as JSON.</summary>
    public string? Data { get; set; }

    /// <summary>The customer or admin who caused the step; null for system and provider events.</summary>
    public Guid? ActorUserId { get; set; }

    /// <summary>UTC.</summary>
    public DateTimeOffset OccurredAt { get; set; }
}
