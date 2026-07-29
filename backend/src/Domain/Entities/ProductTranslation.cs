using MoleculeByMakeover.Domain.Common;

namespace MoleculeByMakeover.Domain.Entities;

public class ProductTranslation : BaseEntity
{
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = default!;

    public int LanguageId { get; set; }
    public Language Language { get; set; } = default!;

    public string Name { get; set; } = default!;
    public string Slug { get; set; } = default!;
    public string? ShortDescription { get; set; }
    public string? Description { get; set; }
    public string? Ingredients { get; set; }
    public string? UsageInstructions { get; set; }
    public string? Benefits { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
}
