namespace MoleculeByMakeover.Infrastructure.Payments;

public class StripeOptions
{
    public const string SectionName = "Payments:Stripe";

    public bool Enabled { get; set; } = true;
    public string SecretKey { get; set; } = default!;
    public string PublishableKey { get; set; } = default!;
    public string WebhookSecret { get; set; } = default!;
}
