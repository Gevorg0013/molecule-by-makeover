using FluentValidation;
using MoleculeByMakeover.Shared.Constants;

namespace MoleculeByMakeover.Application.Features.Content;

public class BlogPostUpsertRequestValidator : AbstractValidator<BlogPostUpsertRequest>
{
    public BlogPostUpsertRequestValidator()
    {
        RuleFor(x => x.Translations).NotEmpty();
        RuleForEach(x => x.Translations).ChildRules(t =>
        {
            t.RuleFor(x => x.LanguageCode).NotEmpty().Must(c => LanguageCodes.All.Contains(c));
            t.RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
            t.RuleFor(x => x.Slug).NotEmpty().MaximumLength(300).Matches("^[a-z0-9-]+$");
            t.RuleFor(x => x.Content).NotEmpty();
        });
    }
}

public class PageUpsertRequestValidator : AbstractValidator<PageUpsertRequest>
{
    public PageUpsertRequestValidator()
    {
        RuleFor(x => x.Key).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Translations).NotEmpty();
        RuleForEach(x => x.Translations).ChildRules(t =>
        {
            t.RuleFor(x => x.LanguageCode).NotEmpty().Must(c => LanguageCodes.All.Contains(c));
            t.RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
            t.RuleFor(x => x.Slug).NotEmpty().MaximumLength(300);
            t.RuleFor(x => x.Content).NotEmpty();
        });
    }
}
