namespace MoleculeByMakeover.Application.Features.Marketing;

public record BannerDto(Guid Id, string ImageUrl, string? LinkUrl, string? Title, string? Subtitle, string? CtaText);

public record BannerTranslationInput(string LanguageCode, string? Title, string? Subtitle, string? CtaText);

public record BannerTranslationDto(Guid Id, string LanguageCode, string? Title, string? Subtitle, string? CtaText);

public record BannerAdminDto(Guid Id, string ImageUrl, string? LinkUrl, int SortOrder, bool IsActive, DateTimeOffset? StartsAt, DateTimeOffset? EndsAt, List<BannerTranslationDto> Translations);

public record BannerUpsertRequest(string ImageUrl, string? LinkUrl, int SortOrder, bool IsActive, DateTimeOffset? StartsAt, DateTimeOffset? EndsAt, List<BannerTranslationInput> Translations);
