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
        RuleFor(x => x.DiscountType).Must(t => t is Domain.Enums.DiscountType.Percent or Domain.Enums.DiscountType.Fixed)
            .WithMessage("Discount type must be Percent or Fixed.");
        RuleFor(x => x.DiscountValue).LessThanOrEqualTo(100)
            .When(x => x.DiscountType == Domain.Enums.DiscountType.Percent)
            .WithMessage("A percentage discount cannot exceed 100.");
        RuleFor(x => x.MinOrderAmount).GreaterThanOrEqualTo(0).When(x => x.MinOrderAmount is not null);
        RuleFor(x => x.MaxUses).GreaterThan(0).When(x => x.MaxUses is not null);
        RuleFor(x => x.ExpiresAt).GreaterThan(x => x.StartsAt)
            .When(x => x.StartsAt is not null && x.ExpiresAt is not null)
            .WithMessage("The expiry date must be after the start date.");
    }
}
