namespace MoleculeByMakeover.Application.Features.Catalog;

public record CategoryDto(
    Guid Id,
    Guid? ParentCategoryId,
    string Name,
    string Slug,
    string? Description,
    string? ImageUrl,
    int SortOrder,
    string? MetaTitle,
    string? MetaDescription,
    List<CategoryDto> Children);

public record CategoryTranslationInput(string LanguageCode, string Name, string Slug, string? Description, string? MetaTitle, string? MetaDescription);

public record CategoryTranslationDto(Guid Id, string LanguageCode, string Name, string Slug, string? Description, string? MetaTitle, string? MetaDescription);

public record CategoryAdminDto(
    Guid Id,
    Guid? ParentCategoryId,
    string? ImageUrl,
    int SortOrder,
    bool IsActive,
    List<CategoryTranslationDto> Translations);

public record CategoryUpsertRequest(
    Guid? ParentCategoryId,
    string? ImageUrl,
    int SortOrder,
    bool IsActive,
    List<CategoryTranslationInput> Translations);
