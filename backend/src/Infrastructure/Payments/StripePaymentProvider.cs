using Microsoft.Extensions.Options;
using MoleculeByMakeover.Application.Common.Interfaces;
using Stripe;

namespace MoleculeByMakeover.Infrastructure.Payments;

// First IPaymentProvider adapter. Idram/ArCa/Telcell become sibling classes later -
// the Checkout/Webhook code in Application never references this type directly,
// only IPaymentProvider, resolved by IPaymentProviderResolver via ProviderKey.
public class StripePaymentProvider(IOptions<StripeOptions> options) : IPaymentProvider
{
    private readonly StripeOptions _options = options.Value;
    private RequestOptions RequestOptions => new() { ApiKey = _options.SecretKey };

    public string ProviderKey => "stripe";

    public async Task<PaymentSessionResult> CreateSessionAsync(PaymentSessionRequest request, CancellationToken ct = default)
    {
        var service = new PaymentIntentService();
        var intent = await service.CreateAsync(new PaymentIntentCreateOptions
        {
            Amount = ToMinorUnits(request.Amount),
            Currency = request.Currency.ToLowerInvariant(),
            ReceiptEmail = string.IsNullOrWhiteSpace(request.CustomerEmail) ? null : request.CustomerEmail,
            Metadata = new Dictionary<string, string> { ["orderNumber"] = request.OrderNumber },
            AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions { Enabled = true }
        }, RequestOptions, ct);

        // Stripe is client-tokenized (Elements/PaymentSheet), so only ClientSecret is set - never RedirectUrl.
        return new PaymentSessionResult(intent.Id, intent.ClientSecret, null);
    }

    public Task<PaymentWebhookResult> HandleWebhookAsync(string rawBody, IDictionary<string, string> headers, CancellationToken ct = default)
    {
        if (!headers.TryGetValue("Stripe-Signature", out var signature))
            return Task.FromResult(new PaymentWebhookResult(false, string.Empty, string.Empty, PaymentEventType.Unknown));

        try
        {
            var stripeEvent = EventUtility.ConstructEvent(rawBody, signature, _options.WebhookSecret);

            var eventType = stripeEvent.Type switch
            {
                "payment_intent.succeeded" => PaymentEventType.PaymentSucceeded,
                "payment_intent.payment_failed" => PaymentEventType.PaymentFailed,
                "charge.refunded" => PaymentEventType.Refunded,
                _ => PaymentEventType.Unknown
            };

            var providerReference = stripeEvent.Data.Object is PaymentIntent intent ? intent.Id : string.Empty;

            return Task.FromResult(new PaymentWebhookResult(true, providerReference, stripeEvent.Id, eventType));
        }
        catch (StripeException)
        {
            return Task.FromResult(new PaymentWebhookResult(false, string.Empty, string.Empty, PaymentEventType.Unknown));
        }
    }

    public async Task<RefundResult> RefundAsync(string providerReference, decimal amount, string currency, CancellationToken ct = default)
    {
        try
        {
            var service = new RefundService();
            var refund = await service.CreateAsync(new RefundCreateOptions
            {
                PaymentIntent = providerReference,
                Amount = ToMinorUnits(amount)
            }, RequestOptions, ct);

            return new RefundResult(true, refund.Id, null);
        }
        catch (StripeException ex)
        {
            return new RefundResult(false, null, ex.Message);
        }
    }

    private static long ToMinorUnits(decimal amount) => (long)Math.Round(amount * 100, MidpointRounding.AwayFromZero);
}
