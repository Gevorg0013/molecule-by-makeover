using MoleculeByMakeover.Domain.Common;

namespace MoleculeByMakeover.Domain.Entities;

public class Banner : BaseEntity
{
    public string ImageUrl { get; set; } = default!;
    public string? LinkUrl { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset? StartsAt { get; set; }
    public DateTimeOffset? EndsAt { get; set; }

    public ICollection<BannerTranslation> Translations { get; set; } = [];
}

public class BannerTranslation : BaseEntity
{
    public Guid BannerId { get; set; }
    public Banner Banner { get; set; } = default!;

    public int LanguageId { get; set; }
    public Language Language { get; set; } = default!;

    public string? Title { get; set; }
    public string? Subtitle { get; set; }
    public string? CtaText { get; set; }
}
