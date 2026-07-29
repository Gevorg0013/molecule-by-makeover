using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MoleculeByMakeover.Application.Common.Interfaces;
using MoleculeByMakeover.Domain.Entities;
using MoleculeByMakeover.Domain.Interfaces;
using MoleculeByMakeover.Shared.Constants;
using MoleculeByMakeover.Shared.Exceptions;

namespace MoleculeByMakeover.Application.Features.Ordering;

public class CheckoutService(
    IUnitOfWork unitOfWork,
    IPaymentProviderResolver paymentProviderResolver,
    IValidator<CheckoutRequest> validator) : ICheckoutService
{
    public async Task<CheckoutResultDto> PlaceOrderAsync(
        Guid userId, string? guestToken, CheckoutRequest request, string returnUrl, string cancelUrl, CancellationToken ct = default)
    {
        await validator.ValidateAndThrowAsync(request, ct);

        var cart = await unitOfWork.Carts.GetActiveByUserIdAsync(userId, ct);
        if (cart is null || cart.Items.Count == 0)
            throw new BadRequestException("Your cart is empty.");

        // Re-validate stock and re-check the coupon server-side; never trust client-computed totals.
        foreach (var item in cart.Items)
        {
            if (item.Quantity > item.Product.Stock)
                throw new BadRequestException($"'{item.Product.Sku}' only has {item.Product.Stock} unit(s) left in stock.");
        }

        var subTotal = cart.Items.Sum(i => i.UnitPriceSnapshot * i.Quantity);
        var discountTotal = 0m;
        if (cart.Coupon is not null)
        {
            if (!cart.Coupon.IsValidNow())
                throw new BadRequestException("The applied coupon is no longer valid.");

            discountTotal = cart.Coupon.DiscountType == Domain.Enums.DiscountType.Percent
                ? Math.Round(subTotal * cart.Coupon.DiscountValue / 100m, 2)
                : Math.Min(subTotal, cart.Coupon.DiscountValue);

            cart.Coupon.UsesCount++;
            unitOfWork.Coupons.Update(cart.Coupon);
        }

        const decimal shippingTotal = 0m; // flat-rate/free shipping for launch; extend with shipping zones later.
        var grandTotal = subTotal - discountTotal + shippingTotal;
        var orderNumber = GenerateOrderNumber();

        // Create the payment session BEFORE writing anything to the database. If the payment
        // provider is unreachable or misconfigured, the customer sees a clean error and their
        // cart/stock are untouched, instead of an order being silently created with no way to pay.
        var provider = paymentProviderResolver.Resolve(request.PaymentProviderKey);
        PaymentSessionResult session;
        try
        {
            session = await provider.CreateSessionAsync(
                new PaymentSessionRequest(orderNumber, grandTotal, StoreDefaults.Currency, string.Empty, returnUrl, cancelUrl), ct);
        }
        catch (Exception ex) when (ex is not BadRequestException)
        {
            throw new BadRequestException("We couldn't start the payment process. Please try again in a moment.");
        }

        var order = new Order
        {
            OrderNumber = orderNumber,
            UserId = userId,
            CouponId = cart.CouponId,
            SubTotal = subTotal,
            DiscountTotal = discountTotal,
            ShippingTotal = shippingTotal,
            GrandTotal = grandTotal,
            Currency = StoreDefaults.Currency,
            ShippingAddress = request.ShippingAddress,
            PaymentProvider = request.PaymentProviderKey,
            PaymentReference = session.ProviderReference,
            PlacedAt = DateTimeOffset.UtcNow,
            StockReservationExpiresAt = DateTimeOffset.UtcNow.AddMinutes(StoreDefaults.StockReservationMinutes)
        };

        foreach (var item in cart.Items)
        {
            order.Items.Add(new OrderItem
            {
                OrderId = order.Id,
                ProductId = item.ProductId,
                ProductNameSnapshot = item.Product.Translations.FirstOrDefault()?.Name ?? item.Product.Sku,
                SkuSnapshot = item.Product.Sku,
                UnitPriceSnapshot = item.UnitPriceSnapshot,
                Quantity = item.Quantity,
                LineTotal = item.UnitPriceSnapshot * item.Quantity
            });

            // Reserve stock immediately; released by the expiry job if payment never completes.
            item.Product.Stock -= item.Quantity;
        }

        await unitOfWork.Orders.AddAsync(order, ct);

        cart.Items.Clear();
        cart.CouponId = null;

        await unitOfWork.SaveChangesAsync(ct);

        return new CheckoutResultDto(order.OrderNumber, session.ClientSecret, session.RedirectUrl, order.GrandTotal, order.Currency);
    }

    private static string GenerateOrderNumber() =>
        $"MBM-{DateTimeOffset.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}";
}
