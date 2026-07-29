using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MoleculeByMakeover.Domain.Entities;

namespace MoleculeByMakeover.Infrastructure.Persistence.Configurations;

public class BannerConfiguration : IEntityTypeConfiguration<Banner>
{
    public void Configure(EntityTypeBuilder<Banner> builder)
    {
        builder.ToTable("Banners");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ImageUrl).IsRequired();
    }
}

public class BannerTranslationConfiguration : IEntityTypeConfiguration<BannerTranslation>
{
    public void Configure(EntityTypeBuilder<BannerTranslation> builder)
    {
        builder.ToTable("BannerTranslations");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.BannerId, x.LanguageId }).IsUnique();

        builder.HasOne(x => x.Banner).WithMany(b => b.Translations)
            .HasForeignKey(x => x.BannerId).OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Language).WithMany()
            .HasForeignKey(x => x.LanguageId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class NewsletterSubscriberConfiguration : IEntityTypeConfiguration<NewsletterSubscriber>
{
    public void Configure(EntityTypeBuilder<NewsletterSubscriber> builder)
    {
        builder.ToTable("NewsletterSubscribers");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Email).HasMaxLength(256).IsRequired();
        builder.HasIndex(x => x.Email).IsUnique();
    }
}
