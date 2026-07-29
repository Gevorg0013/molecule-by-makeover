using MoleculeByMakeover.Domain.Common;

namespace MoleculeByMakeover.Domain.Entities;

public class CategoryTranslation : BaseEntity
{
    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = default!;

    public int LanguageId { get; set; }
    public Language Language { get; set; } = default!;

    public string Name { get; set; } = default!;
    public string Slug { get; set; } = default!;
    public string? Description { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
}
