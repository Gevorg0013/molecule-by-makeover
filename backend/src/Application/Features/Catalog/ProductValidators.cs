using FluentValidation;
using MoleculeByMakeover.Shared.Constants;

namespace MoleculeByMakeover.Application.Features.Catalog;

public class ProductUpsertRequestValidator : AbstractValidator<ProductUpsertRequest>
{
    public ProductUpsertRequestValidator()
    {
        RuleFor(x => x.Sku).NotEmpty().MaximumLength(64);
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.Price).GreaterThan(0);
        RuleFor(x => x.Stock).GreaterThanOrEqualTo(0);
        RuleFor(x => x.DiscountValue)
            .NotNull().When(x => x.DiscountType != Domain.Enums.DiscountType.None)
            .WithMessage("Discount value is required when a discount type is set.");

        RuleFor(x => x.Translations).NotEmpty();
        RuleForEach(x => x.Translations).ChildRules(t =>
        {
            t.RuleFor(x => x.LanguageCode).NotEmpty().Must(c => LanguageCodes.All.Contains(c));
            t.RuleFor(x => x.Name).NotEmpty().MaximumLength(300);
            t.RuleFor(x => x.Slug)
                .NotEmpty()
                .MaximumLength(300)
                .Matches(@"^[\p{L}\p{N}-]+$")
                .WithMessage("Slug can contain only letters, numbers, and hyphens.");
        });
    }
}
