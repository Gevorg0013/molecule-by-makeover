using FluentValidation;
using MoleculeByMakeover.Shared.Constants;

namespace MoleculeByMakeover.Application.Features.Marketing;

public class BannerUpsertRequestValidator : AbstractValidator<BannerUpsertRequest>
{
    public BannerUpsertRequestValidator()
    {
        RuleFor(x => x.ImageUrl).NotEmpty();
        RuleForEach(x => x.Translations).ChildRules(t =>
        {
            t.RuleFor(x => x.LanguageCode).NotEmpty().Must(c => LanguageCodes.All.Contains(c));
        });
    }
}

public class NewsletterSubscribeRequestValidator : AbstractValidator<NewsletterSubscribeRequest>
{
    public NewsletterSubscribeRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}
