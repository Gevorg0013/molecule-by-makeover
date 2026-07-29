using Microsoft.AspNetCore.Mvc;
using MoleculeByMakeover.Application.Common.Interfaces;
using MoleculeByMakeover.Application.Features.Ordering;
using MoleculeByMakeover.Domain.Entities;
using MoleculeByMakeover.Domain.Interfaces;

namespace MoleculeByMakeover.API.Controllers;

public class PaymentsController(
    IPaymentProviderResolver paymentProviderResolver,
    IOrderService orderService,
    IUnitOfWork unitOfWork) : ApiControllerBase
{
    [HttpPost("webhook/{provider}")]
    public async Task<IActionResult> Webhook(string provider, CancellationToken ct)
    {
        using var reader = new StreamReader(Request.Body);
        var rawBody = await reader.ReadToEndAsync(ct);
        var headers = Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString());

        var paymentProvider = paymentProviderResolver.Resolve(provider);
        var result = await paymentProvider.HandleWebhookAsync(rawBody, headers, ct);

        if (!result.IsValid)
            return BadRequest();

        // Idempotency: a retried webhook delivery for an already-processed event is a no-op.
        if (await unitOfWork.ProcessedPaymentEvents.HasBeenProcessedAsync(provider, result.EventId, ct))
            return Ok();

        if (result.EventType == PaymentEventType.PaymentSucceeded)
            await orderService.MarkPaidAsync(result.ProviderReference, ct);

        await unitOfWork.ProcessedPaymentEvents.AddAsync(new ProcessedPaymentEvent
        {
            ProviderKey = provider,
            EventId = result.EventId,
            ProcessedAt = DateTimeOffset.UtcNow
        }, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return Ok();
    }
}
