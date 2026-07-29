using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MoleculeByMakeover.Application.Common.Interfaces;
using MoleculeByMakeover.Domain.Entities;
using MoleculeByMakeover.Domain.Interfaces;
using MoleculeByMakeover.Shared.Exceptions;

namespace MoleculeByMakeover.Application.Features.Catalog;

public class CategoryService(
    IUnitOfWork unitOfWork,
    ICurrentLanguageService currentLanguage,
    IValidator<CategoryUpsertRequest> validator) : ICategoryService
{
    public async Task<List<CategoryDto>> GetTreeAsync(CancellationToken ct = default)
    {
        var categories = await unitOfWork.Categories.QueryWithTranslations()
            .Where(c => c.IsActive && !c.IsDeleted)
            .OrderBy(c => c.SortOrder)
            .ToListAsync(ct);

        var byParent = categories.ToLookup(c => c.ParentCategoryId);

        List<CategoryDto> BuildLevel(Guid? parentId) =>
            byParent[parentId]
                .Select(c => ToDto(c, BuildLevel(c.Id)))
                .ToList();

        return BuildLevel(null);
    }

    public async Task<CategoryDto> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        var category = await unitOfWork.Categories.GetBySlugAsync(slug, currentLanguage.LanguageId, ct)
            ?? throw new NotFoundException(nameof(Category), slug);

        return ToDto(category, []);
    }

    public async Task<List<CategoryAdminDto>> GetAllAdminAsync(CancellationToken ct = default)
    {
        var categories = await unitOfWork.Categories.QueryWithTranslations()
            .Where(c => !c.IsDeleted)
            .OrderBy(c => c.SortOrder)
            .ToListAsync(ct);

        return categories.Select(ToAdminDto).ToList();
    }

    public async Task<CategoryAdminDto> GetByIdAdminAsync(Guid id, CancellationToken ct = default)
    {
        var category = await unitOfWork.Categories.QueryWithTranslations()
            .FirstOrDefaultAsync(c => c.Id == id, ct)
            ?? throw new NotFoundException(nameof(Category), id);

        return ToAdminDto(category);
    }

    public async Task<Guid> CreateAsync(CategoryUpsertRequest request, CancellationToken ct = default)
    {
        await validator.ValidateAndThrowAsync(request, ct);

        var category = new Category
        {
            ParentCategoryId = request.ParentCategoryId,
            ImageUrl = request.ImageUrl,
            SortOrder = request.SortOrder,
            IsActive = request.IsActive,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await ApplyTranslationsAsync(category, request.Translations, ct);

        await unitOfWork.Categories.AddAsync(category, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return category.Id;
    }

    public async Task UpdateAsync(Guid id, CategoryUpsertRequest request, CancellationToken ct = default)
    {
        await validator.ValidateAndThrowAsync(request, ct);

        var category = await unitOfWork.Categories.QueryWithTranslations()
            .FirstOrDefaultAsync(c => c.Id == id, ct)
            ?? throw new NotFoundException(nameof(Category), id);

        category.ParentCategoryId = request.ParentCategoryId;
        category.ImageUrl = request.ImageUrl;
        category.SortOrder = request.SortOrder;
        category.IsActive = request.IsActive;
        category.UpdatedAt = DateTimeOffset.UtcNow;

        category.Translations.Clear();
        await ApplyTranslationsAsync(category, request.Translations, ct);

        unitOfWork.Categories.Update(category);
        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var category = await unitOfWork.Categories.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Category), id);

        category.IsDeleted = true;
        category.DeletedAt = DateTimeOffset.UtcNow;
        unitOfWork.Categories.Update(category);
        await unitOfWork.SaveChangesAsync(ct);
    }

    private async Task ApplyTranslationsAsync(Category category, List<CategoryTranslationInput> translations, CancellationToken ct)
    {
        foreach (var t in translations)
        {
            var language = await unitOfWork.Languages.GetByCodeAsync(t.LanguageCode, ct)
                ?? throw new BadRequestException($"Unknown language code '{t.LanguageCode}'.");

            var translation = new CategoryTranslation
            {
                LanguageId = language.Id,
                Name = t.Name,
                Slug = t.Slug,
                Description = t.Description,
                MetaTitle = t.MetaTitle,
                MetaDescription = t.MetaDescription,
                CreatedAt = DateTimeOffset.UtcNow
            };
            category.Translations.Add(translation);
            unitOfWork.TrackNew(translation);
        }
    }

    private CategoryDto ToDto(Category category, List<CategoryDto> children)
    {
        var translation = category.Translations.FirstOrDefault(t => t.LanguageId == currentLanguage.LanguageId)
            ?? category.Translations.FirstOrDefault();

        return new CategoryDto(
            category.Id,
            category.ParentCategoryId,
            translation?.Name ?? string.Empty,
            translation?.Slug ?? string.Empty,
            translation?.Description,
            category.ImageUrl,
            category.SortOrder,
            translation?.MetaTitle,
            translation?.MetaDescription,
            children);
    }

    private static CategoryAdminDto ToAdminDto(Category category) => new(
        category.Id,
        category.ParentCategoryId,
        category.ImageUrl,
        category.SortOrder,
        category.IsActive,
        category.Translations.Select(t => new CategoryTranslationDto(
            t.Id, t.Language.Code, t.Name, t.Slug, t.Description, t.MetaTitle, t.MetaDescription)).ToList());
}
