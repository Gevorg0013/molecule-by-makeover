using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MoleculeByMakeover.Application.Common.Interfaces;
using MoleculeByMakeover.Domain.Entities;
using MoleculeByMakeover.Domain.Interfaces;
using MoleculeByMakeover.Shared.Exceptions;

namespace MoleculeByMakeover.Application.Features.Marketing;

public class BannerService(
    IUnitOfWork unitOfWork,
    ICurrentLanguageService currentLanguage,
    IValidator<BannerUpsertRequest> validator) : IBannerService
{
    public async Task<List<BannerDto>> GetActiveAsync(CancellationToken ct = default)
    {
        var banners = await unitOfWork.Banners.GetActiveAsync(ct);
        return banners.Select(ToDto).ToList();
    }

    public async Task<List<BannerAdminDto>> GetAllAdminAsync(CancellationToken ct = default)
    {
        var banners = await unitOfWork.Banners.Query().Include(b => b.Translations).ThenInclude(t => t.Language)
            .OrderBy(b => b.SortOrder).ToListAsync(ct);
        return banners.Select(ToAdminDto).ToList();
    }

    public async Task<Guid> CreateAsync(BannerUpsertRequest request, CancellationToken ct = default)
    {
        await validator.ValidateAndThrowAsync(request, ct);

        var banner = new Banner
        {
            ImageUrl = request.ImageUrl,
            LinkUrl = request.LinkUrl,
            SortOrder = request.SortOrder,
            IsActive = request.IsActive,
            StartsAt = request.StartsAt,
            EndsAt = request.EndsAt,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await ApplyTranslationsAsync(banner, request.Translations, ct);
        await unitOfWork.Banners.AddAsync(banner, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return banner.Id;
    }

    public async Task UpdateAsync(Guid id, BannerUpsertRequest request, CancellationToken ct = default)
    {
        await validator.ValidateAndThrowAsync(request, ct);

        var banner = await unitOfWork.Banners.Query().Include(b => b.Translations)
            .FirstOrDefaultAsync(b => b.Id == id, ct) ?? throw new NotFoundException(nameof(Banner), id);

        banner.ImageUrl = request.ImageUrl;
        banner.LinkUrl = request.LinkUrl;
        banner.SortOrder = request.SortOrder;
        banner.IsActive = request.IsActive;
        banner.StartsAt = request.StartsAt;
        banner.EndsAt = request.EndsAt;
        banner.UpdatedAt = DateTimeOffset.UtcNow;

        banner.Translations.Clear();
        await ApplyTranslationsAsync(banner, request.Translations, ct);

        unitOfWork.Banners.Update(banner);
        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var banner = await unitOfWork.Banners.GetByIdAsync(id, ct) ?? throw new NotFoundException(nameof(Banner), id);
        unitOfWork.Banners.Remove(banner);
        await unitOfWork.SaveChangesAsync(ct);
    }

    private async Task ApplyTranslationsAsync(Banner banner, List<BannerTranslationInput> translations, CancellationToken ct)
    {
        foreach (var t in translations)
        {
            var language = await unitOfWork.Languages.GetByCodeAsync(t.LanguageCode, ct)
                ?? throw new BadRequestException($"Unknown language code '{t.LanguageCode}'.");

            var translation = new BannerTranslation
            {
                LanguageId = language.Id,
                Title = t.Title,
                Subtitle = t.Subtitle,
                CtaText = t.CtaText,
                CreatedAt = DateTimeOffset.UtcNow
            };
            banner.Translations.Add(translation);
            unitOfWork.TrackNew(translation);
        }
    }

    private BannerDto ToDto(Banner banner)
    {
        var translation = ResolveTranslation(banner.Translations);
        return new BannerDto(banner.Id, banner.ImageUrl, banner.LinkUrl, translation?.Title, translation?.Subtitle, translation?.CtaText);
    }

    private static BannerAdminDto ToAdminDto(Banner banner) => new(
        banner.Id, banner.ImageUrl, banner.LinkUrl, banner.SortOrder, banner.IsActive, banner.StartsAt, banner.EndsAt,
        banner.Translations.Select(t => new BannerTranslationDto(t.Id, t.Language.Code, t.Title, t.Subtitle, t.CtaText)).ToList());

    private BannerTranslation? ResolveTranslation(IEnumerable<BannerTranslation> translations) =>
        translations.FirstOrDefault(t => t.LanguageId == currentLanguage.LanguageId) ?? translations.FirstOrDefault();
}

public class NewsletterService(IUnitOfWork unitOfWork, IValidator<NewsletterSubscribeRequest> validator) : INewsletterService
{
    public async Task SubscribeAsync(NewsletterSubscribeRequest request, CancellationToken ct = default)
    {
        await validator.ValidateAndThrowAsync(request, ct);

        var email = request.Email.Trim().ToLowerInvariant();
        if (await unitOfWork.NewsletterSubscribers.ExistsAsync(email, ct)) return;

        await unitOfWork.NewsletterSubscribers.AddAsync(new Domain.Entities.NewsletterSubscriber
        {
            Email = email,
            SubscribedAt = DateTimeOffset.UtcNow
        }, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
