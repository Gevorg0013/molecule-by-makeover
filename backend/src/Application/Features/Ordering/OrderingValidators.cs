using FluentValidation;

namespace MoleculeByMakeover.Application.Features.Ordering;

public class AddCartItemRequestValidator : AbstractValidator<AddCartItemRequest>
{
    public AddCartItemRequestValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0).LessThanOrEqualTo(100);
    }
}

public class UpdateCartItemRequestValidator : AbstractValidator<UpdateCartItemRequest>
{
    public UpdateCartItemRequestValidator()
    {
        RuleFor(x => x.Quantity).GreaterThan(0).LessThanOrEqualTo(100);
    }
}

public class CheckoutRequestValidator : AbstractValidator<CheckoutRequest>
{
    public CheckoutRequestValidator()
    {
        RuleFor(x => x.PaymentProviderKey).NotEmpty();
        RuleFor(x => x.ShippingAddress).NotNull();
        RuleFor(x => x.ShippingAddress.FullName).NotEmpty().When(x => x.ShippingAddress is not null);
        RuleFor(x => x.ShippingAddress.Phone).NotEmpty().When(x => x.ShippingAddress is not null);
        RuleFor(x => x.ShippingAddress.Country).NotEmpty().When(x => x.ShippingAddress is not null);
        RuleFor(x => x.ShippingAddress.City).NotEmpty().When(x => x.ShippingAddress is not null);
        RuleFor(x => x.ShippingAddress.AddressLine1).NotEmpty().When(x => x.ShippingAddress is not null);
    }
}

public class CouponUpsertRequestValidator : AbstractValidator<CouponUpsertRequest>
{
    public CouponUpsertRequestValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.DiscountValue).GreaterThan(0);
    }
}
