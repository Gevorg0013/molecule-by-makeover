namespace MoleculeByMakeover.Application.Features.Ordering;

public interface ICheckoutService
{
    Task<CheckoutResultDto> PlaceOrderAsync(Guid userId, string? guestToken, CheckoutRequest request, string returnUrl, string cancelUrl, CancellationToken ct = default);
}
