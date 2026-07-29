namespace MoleculeByMakeover.Application.Common.Interfaces;

/// <summary>
/// Abstraction over any payment gateway (Stripe today; Idram/ArCa/Telcell later).
/// The Application layer only ever talks to this interface, never to a concrete SDK,
/// so adding a provider is a new Infrastructure adapter + DI registration - no changes here.
/// </summary>
public interface IPaymentProvider
{
    /// <summary>Config/DI key this provider is registered under, e.g. "stripe", "idram", "arca", "telcell".</summary>
    string ProviderKey { get; }

    Task<PaymentSessionResult> CreateSessionAsync(PaymentSessionRequest request, CancellationToken ct = default);

    /// <summary>
    /// Verifies and parses a raw webhook payload for this provider. Each adapter owns its
    /// own signature/HMAC verification scheme internally and returns a normalized result.
    /// </summary>
    Task<PaymentWebhookResult> HandleWebhookAsync(string rawBody, IDictionary<string, string> headers, CancellationToken ct = default);

    Task<RefundResult> RefundAsync(string providerReference, decimal amount, string currency, CancellationToken ct = default);
}

public record PaymentSessionRequest(
    string OrderNumber,
    decimal Amount,
    string Currency,
    string CustomerEmail,
    string ReturnUrl,
    string CancelUrl);

/// <summary>
/// Exactly one of ClientSecret (client-tokenized flows, e.g. Stripe) or RedirectUrl
/// (hosted-page flows, e.g. Idram/ArCa/Telcell) is populated - the Checkout use case
/// and the frontend branch on whichever is set, never on the provider name.
/// </summary>
public record PaymentSessionResult(string ProviderReference, string? ClientSecret, string? RedirectUrl);

public record PaymentWebhookResult(bool IsValid, string ProviderReference, string EventId, PaymentEventType EventType);

public enum PaymentEventType
{
    Unknown = 0,
    PaymentSucceeded = 1,
    PaymentFailed = 2,
    Refunded = 3
}

public record RefundResult(bool Succeeded, string? ProviderRefundReference, string? Error);

/// <summary>Resolves the active IPaymentProvider by its config/DI key at checkout/webhook time.</summary>
public interface IPaymentProviderResolver
{
    IPaymentProvider Resolve(string providerKey);
}
