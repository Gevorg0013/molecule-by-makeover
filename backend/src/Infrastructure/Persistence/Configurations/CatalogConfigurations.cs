using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MoleculeByMakeover.Domain.Entities;

namespace MoleculeByMakeover.Infrastructure.Persistence.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");
        builder.HasKey(x => x.Id);
        builder.HasQueryFilter(x => !x.IsDeleted);

        builder.HasOne(x => x.ParentCategory).WithMany(x => x.ChildCategories)
            .HasForeignKey(x => x.ParentCategoryId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class CategoryTranslationConfiguration : IEntityTypeConfiguration<CategoryTranslation>
{
    public void Configure(EntityTypeBuilder<CategoryTranslation> builder)
    {
        builder.ToTable("CategoryTranslations");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Slug).HasMaxLength(200).IsRequired();
        builder.HasIndex(x => new { x.CategoryId, x.LanguageId }).IsUnique();
        builder.HasIndex(x => new { x.Slug, x.LanguageId }).IsUnique();
        builder.HasQueryFilter(x => !x.Category.IsDeleted);

        builder.HasOne(x => x.Category).WithMany(c => c.Translations)
            .HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Language).WithMany()
            .HasForeignKey(x => x.LanguageId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");
        builder.HasKey(x => x.Id);
        builder.HasQueryFilter(x => !x.IsDeleted);

        builder.Property(x => x.Sku).HasMaxLength(64).IsRequired();
        builder.HasIndex(x => x.Sku).IsUnique();
        builder.Property(x => x.Price).HasColumnType("numeric(18,2)");
        builder.Property(x => x.DiscountValue).HasColumnType("numeric(18,2)");
        builder.Property(x => x.Brand).HasMaxLength(150);
        builder.Ignore(x => x.FinalPrice);

        builder.HasOne(x => x.Category).WithMany(c => c.Products)
            .HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class ProductTranslationConfiguration : IEntityTypeConfiguration<ProductTranslation>
{
    public void Configure(EntityTypeBuilder<ProductTranslation> builder)
    {
        builder.ToTable("ProductTranslations");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(300).IsRequired();
        builder.Property(x => x.Slug).HasMaxLength(300).IsRequired();
        builder.HasIndex(x => new { x.ProductId, x.LanguageId }).IsUnique();
        builder.HasIndex(x => new { x.Slug, x.LanguageId }).IsUnique();
        builder.HasQueryFilter(x => !x.Product.IsDeleted);

        builder.HasOne(x => x.Product).WithMany(p => p.Translations)
            .HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Language).WithMany()
            .HasForeignKey(x => x.LanguageId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
{
    public void Configure(EntityTypeBuilder<ProductImage> builder)
    {
        builder.ToTable("ProductImages");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Url).IsRequired();
        builder.HasQueryFilter(x => !x.Product.IsDeleted);

        builder.HasOne(x => x.Product).WithMany(p => p.Images)
            .HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.ToTable("Tags");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.HasIndex(x => x.Name).IsUnique();
    }
}

public class ProductTagConfiguration : IEntityTypeConfiguration<ProductTag>
{
    public void Configure(EntityTypeBuilder<ProductTag> builder)
    {
        builder.ToTable("ProductTags");
        builder.HasKey(x => new { x.ProductId, x.TagId });
        builder.HasQueryFilter(x => !x.Product.IsDeleted);

        builder.HasOne(x => x.Product).WithMany(p => p.ProductTags)
            .HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Tag).WithMany(t => t.ProductTags)
            .HasForeignKey(x => x.TagId).OnDelete(DeleteBehavior.Cascade);
    }
}
