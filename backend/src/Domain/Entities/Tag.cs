using MoleculeByMakeover.Domain.Common;

namespace MoleculeByMakeover.Domain.Entities;

public class Tag : BaseEntity
{
    public string Name { get; set; } = default!;

    public ICollection<ProductTag> ProductTags { get; set; } = [];
}

public class ProductTag
{
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = default!;

    public Guid TagId { get; set; }
    public Tag Tag { get; set; } = default!;
}
