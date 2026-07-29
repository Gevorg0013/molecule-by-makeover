namespace MoleculeByMakeover.Application.Features.Content;

public record BlogPostListItemDto(Guid Id, string Title, string Slug, string? Excerpt, string? CoverImageUrl, DateTimeOffset? PublishedAt);

public record BlogPostDetailDto(Guid Id, string Title, string Slug, string? Excerpt, string Content, string? CoverImageUrl, string? MetaTitle, string? MetaDescription, DateTimeOffset? PublishedAt);

public record BlogTranslationInput(string LanguageCode, string Title, string Slug, string? Excerpt, string Content, string? MetaTitle, string? MetaDescription);

public record BlogTranslationDto(Guid Id, string LanguageCode, string Title, string Slug, string? Excerpt, string Content, string? MetaTitle, string? MetaDescription);

public record BlogPostAdminDto(Guid Id, string? CoverImageUrl, bool IsPublished, DateTimeOffset? PublishedAt, List<BlogTranslationDto> Translations);

public record BlogPostUpsertRequest(string? CoverImageUrl, bool IsPublished, List<BlogTranslationInput> Translations);
