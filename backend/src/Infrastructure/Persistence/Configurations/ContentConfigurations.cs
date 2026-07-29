using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MoleculeByMakeover.Domain.Entities;

namespace MoleculeByMakeover.Infrastructure.Persistence.Configurations;

public class BlogPostConfiguration : IEntityTypeConfiguration<BlogPost>
{
    public void Configure(EntityTypeBuilder<BlogPost> builder)
    {
        builder.ToTable("BlogPosts");
        builder.HasKey(x => x.Id);
        builder.HasQueryFilter(x => !x.IsDeleted);

        builder.HasOne(x => x.Author).WithMany()
            .HasForeignKey(x => x.AuthorId).OnDelete(DeleteBehavior.SetNull);
    }
}

public class BlogPostTranslationConfiguration : IEntityTypeConfiguration<BlogPostTranslation>
{
    public void Configure(EntityTypeBuilder<BlogPostTranslation> builder)
    {
        builder.ToTable("BlogPostTranslations");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Title).HasMaxLength(300).IsRequired();
        builder.Property(x => x.Slug).HasMaxLength(300).IsRequired();
        builder.HasIndex(x => new { x.BlogPostId, x.LanguageId }).IsUnique();
        builder.HasIndex(x => new { x.Slug, x.LanguageId }).IsUnique();
        builder.HasQueryFilter(x => !x.BlogPost.IsDeleted);

        builder.HasOne(x => x.BlogPost).WithMany(b => b.Translations)
            .HasForeignKey(x => x.BlogPostId).OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Language).WithMany()
            .HasForeignKey(x => x.LanguageId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class PageConfiguration : IEntityTypeConfiguration<Page>
{
    public void Configure(EntityTypeBuilder<Page> builder)
    {
        builder.ToTable("Pages");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Key).HasMaxLength(100).IsRequired();
        builder.HasIndex(x => x.Key).IsUnique();
    }
}

public class PageTranslationConfiguration : IEntityTypeConfiguration<PageTranslation>
{
    public void Configure(EntityTypeBuilder<PageTranslation> builder)
    {
        builder.ToTable("PageTranslations");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Title).HasMaxLength(300).IsRequired();
        builder.Property(x => x.Slug).HasMaxLength(300).IsRequired();
        builder.HasIndex(x => new { x.PageId, x.LanguageId }).IsUnique();
        builder.HasIndex(x => new { x.Slug, x.LanguageId }).IsUnique();

        builder.HasOne(x => x.Page).WithMany(p => p.Translations)
            .HasForeignKey(x => x.PageId).OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Language).WithMany()
            .HasForeignKey(x => x.LanguageId).OnDelete(DeleteBehavior.Restrict);
    }
}
