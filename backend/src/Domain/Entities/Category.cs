using MoleculeByMakeover.Domain.Common;

namespace MoleculeByMakeover.Domain.Entities;

public class Category : BaseEntity, ISoftDelete
{
    public Guid? ParentCategoryId { get; set; }
    public string? ImageUrl { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }

    public Category? ParentCategory { get; set; }
    public ICollection<Category> ChildCategories { get; set; } = [];
    public ICollection<CategoryTranslation> Translations { get; set; } = [];
    public ICollection<Product> Products { get; set; } = [];
}
