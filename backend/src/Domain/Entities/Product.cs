using MoleculeByMakeover.Domain.Common;
using MoleculeByMakeover.Domain.Enums;

namespace MoleculeByMakeover.Domain.Entities;

public class Product : BaseEntity, ISoftDelete
{
    public string Sku { get; set; } = default!;
    public Guid CategoryId { get; set; }
    public decimal Price { get; set; }
    public DiscountType DiscountType { get; set; } = DiscountType.None;
    public decimal? DiscountValue { get; set; }
    public int Stock { get; set; }
    public string? MainImageUrl { get; set; }
    public string? Brand { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsBestSeller { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }

    public Category Category { get; set; } = default!;
    public ICollection<ProductTranslation> Translations { get; set; } = [];
    public ICollection<ProductImage> Images { get; set; } = [];
    public ICollection<ProductTag> ProductTags { get; set; } = [];
    public ICollection<Review> Reviews { get; set; } = [];

    public decimal FinalPrice => DiscountType switch
    {
        DiscountType.Percent => Math.Round(Price - Price * (DiscountValue ?? 0) / 100m, 2),
        DiscountType.Fixed => Math.Max(0, Price - (DiscountValue ?? 0)),
        _ => Price
    };
}
