using MoleculeByMakeover.Domain.Common;

namespace MoleculeByMakeover.Domain.Entities;

public class NewsletterSubscriber : BaseEntity
{
    public string Email { get; set; } = default!;
    public DateTimeOffset SubscribedAt { get; set; } = DateTimeOffset.UtcNow;
    public bool IsConfirmed { get; set; }
}
