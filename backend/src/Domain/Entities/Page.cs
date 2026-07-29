using MoleculeByMakeover.Domain.Common;

namespace MoleculeByMakeover.Domain.Entities;

public class Page : BaseEntity
{
    public string Key { get; set; } = default!;
    public bool IsPublished { get; set; } = true;

    public ICollection<PageTranslation> Translations { get; set; } = [];
}

public class PageTranslation : BaseEntity
{
    public Guid PageId { get; set; }
    public Page Page { get; set; } = default!;

    public int LanguageId { get; set; }
    public Language Language { get; set; } = default!;

    public string Title { get; set; } = default!;
    public string Slug { get; set; } = default!;
    public string Content { get; set; } = default!;
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
}
