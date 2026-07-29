using FluentValidation;
using MoleculeByMakeover.Shared.Constants;

namespace MoleculeByMakeover.Application.Features.Catalog;

public class CategoryUpsertRequestValidator : AbstractValidator<CategoryUpsertRequest>
{
    public CategoryUpsertRequestValidator()
    {
        RuleFor(x => x.Translations).NotEmpty()
            .WithMessage("At least one translation is required.");

        RuleForEach(x => x.Translations).ChildRules(t =>
        {
            t.RuleFor(x => x.LanguageCode).NotEmpty().Must(c => LanguageCodes.All.Contains(c))
                .WithMessage($"Language code must be one of: {string.Join(", ", LanguageCodes.All)}");
            t.RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            t.RuleFor(x => x.Slug).NotEmpty().MaximumLength(200).Matches("^[a-z0-9-]+$")
                .WithMessage("Slug must be lowercase alphanumeric with hyphens only.");
        });
    }
}
