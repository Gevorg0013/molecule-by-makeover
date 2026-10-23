using FluentValidation;
using MoleculeByMakeover.Shared.Constants;

namespace MoleculeByMakeover.Application.Features.Catalog;

public class ProductUpsertRequestValidator : AbstractValidator<ProductUpsertRequest>
{
    public ProductUpsertRequestValidator()
    {
        RuleFor(x => x.Sku).NotEmpty().MaximumLength(64).WithName("Product Code");
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.Price).GreaterThan(0);
        RuleFor(x => x.Stock).GreaterThanOrEqualTo(0);
        RuleFor(x => x.DiscountValue)
            .NotNull().When(x => x.DiscountType != Domain.Enums.DiscountType.None)
            .WithMessage("Discount value is required when a discount type is set.");

        RuleFor(x => x.DiscountValue).GreaterThan(0)
            .When(x => x.DiscountType != Domain.Enums.DiscountType.None && x.DiscountValue is not null);
        RuleFor(x => x.DiscountValue).LessThanOrEqualTo(100)
            .When(x => x.DiscountType == Domain.Enums.DiscountType.Percent && x.DiscountValue is not null)
            .WithMessage("A percentage discount cannot exceed 100.");
        RuleFor(x => x.DiscountValue).LessThan(x => x.Price)
            .When(x => x.DiscountType == Domain.Enums.DiscountType.Fixed && x.DiscountValue is not null)
            .WithMessage("A fixed discount must be less than the price.");

        RuleFor(x => x.Translations).NotEmpty();
        RuleForEach(x => x.Translations).ChildRules(t =>
        {
            t.RuleFor(x => x.LanguageCode).NotEmpty().Must(c => LanguageCodes.All.Contains(c));
            t.RuleFor(x => x.Name).NotEmpty().MaximumLength(300);
            t.RuleFor(x => x.Slug)
                .MaximumLength(300)
                .Matches(@"^[\p{L}\p{N}-]+$")
                .WithMessage("Slug can contain only letters, numbers, and hyphens.")
                .When(x => !string.IsNullOrWhiteSpace(x.Slug));
        });
    }
}
