namespace MoleculeByMakeover.Application.Features.Content;

public record PageDto(string Key, string Title, string Slug, string Content, string? MetaTitle, string? MetaDescription);

public record PageTranslationInput(string LanguageCode, string Title, string Slug, string Content, string? MetaTitle, string? MetaDescription);

public record PageTranslationDto(Guid Id, string LanguageCode, string Title, string Slug, string Content, string? MetaTitle, string? MetaDescription);

public record PageAdminDto(Guid Id, string Key, bool IsPublished, List<PageTranslationDto> Translations);

public record PageUpsertRequest(string Key, bool IsPublished, List<PageTranslationInput> Translations);
