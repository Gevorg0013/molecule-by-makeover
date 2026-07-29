using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MoleculeByMakeover.Application.Common.Interfaces;
using MoleculeByMakeover.Domain.Entities;
using MoleculeByMakeover.Domain.Interfaces;
using MoleculeByMakeover.Shared.Exceptions;

namespace MoleculeByMakeover.Application.Features.Content;

public class PageService(
    IUnitOfWork unitOfWork,
    ICurrentLanguageService currentLanguage,
    IValidator<PageUpsertRequest> validator) : IPageService
{
    public async Task<PageDto> GetByKeyAsync(string key, CancellationToken ct = default)
    {
        var page = await unitOfWork.Pages.GetByKeyAsync(key, ct) ?? throw new NotFoundException(nameof(Page), key);
        var translation = ResolveTranslation(page.Translations)
            ?? throw new NotFoundException(nameof(PageTranslation), key);

        return new PageDto(page.Key, translation.Title, translation.Slug, translation.Content, translation.MetaTitle, translation.MetaDescription);
    }

    public async Task<List<PageAdminDto>> GetAllAdminAsync(CancellationToken ct = default)
    {
        var pages = await unitOfWork.Pages.Query().Include(p => p.Translations).ThenInclude(t => t.Language).ToListAsync(ct);
        return pages.Select(ToAdminDto).ToList();
    }

    public async Task<PageAdminDto> GetByIdAdminAsync(Guid id, CancellationToken ct = default)
    {
        var page = await unitOfWork.Pages.Query().Include(p => p.Translations).ThenInclude(t => t.Language)
            .FirstOrDefaultAsync(p => p.Id == id, ct) ?? throw new NotFoundException(nameof(Page), id);
        return ToAdminDto(page);
    }

    public async Task<Guid> CreateAsync(PageUpsertRequest request, CancellationToken ct = default)
    {
        await validator.ValidateAndThrowAsync(request, ct);

        if (await unitOfWork.Pages.GetByKeyAsync(request.Key, ct) is not null)
            throw new ConflictException($"A page with key '{request.Key}' already exists.");

        var page = new Page { Key = request.Key, IsPublished = request.IsPublished, CreatedAt = DateTimeOffset.UtcNow };
        await ApplyTranslationsAsync(page, request.Translations, ct);

        await unitOfWork.Pages.AddAsync(page, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return page.Id;
    }

    public async Task UpdateAsync(Guid id, PageUpsertRequest request, CancellationToken ct = default)
    {
        await validator.ValidateAndThrowAsync(request, ct);

        var page = await unitOfWork.Pages.Query().Include(p => p.Translations)
            .FirstOrDefaultAsync(p => p.Id == id, ct) ?? throw new NotFoundException(nameof(Page), id);

        page.Key = request.Key;
        page.IsPublished = request.IsPublished;
        page.UpdatedAt = DateTimeOffset.UtcNow;

        page.Translations.Clear();
        await ApplyTranslationsAsync(page, request.Translations, ct);

        unitOfWork.Pages.Update(page);
        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var page = await unitOfWork.Pages.GetByIdAsync(id, ct) ?? throw new NotFoundException(nameof(Page), id);
        unitOfWork.Pages.Remove(page);
        await unitOfWork.SaveChangesAsync(ct);
    }

    private async Task ApplyTranslationsAsync(Page page, List<PageTranslationInput> translations, CancellationToken ct)
    {
        foreach (var t in translations)
        {
            var language = await unitOfWork.Languages.GetByCodeAsync(t.LanguageCode, ct)
                ?? throw new BadRequestException($"Unknown language code '{t.LanguageCode}'.");

            var translation = new PageTranslation
            {
                LanguageId = language.Id,
                Title = t.Title,
                Slug = t.Slug,
                Content = t.Content,
                MetaTitle = t.MetaTitle,
                MetaDescription = t.MetaDescription,
                CreatedAt = DateTimeOffset.UtcNow
            };
            page.Translations.Add(translation);
            unitOfWork.TrackNew(translation);
        }
    }

    private static PageAdminDto ToAdminDto(Page page) => new(
        page.Id, page.Key, page.IsPublished,
        page.Translations.Select(t => new PageTranslationDto(t.Id, t.Language.Code, t.Title, t.Slug, t.Content, t.MetaTitle, t.MetaDescription)).ToList());

    private PageTranslation? ResolveTranslation(IEnumerable<PageTranslation> translations) =>
        translations.FirstOrDefault(t => t.LanguageId == currentLanguage.LanguageId) ?? translations.FirstOrDefault();
}
