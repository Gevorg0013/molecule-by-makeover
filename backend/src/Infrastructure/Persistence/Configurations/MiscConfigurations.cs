using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MoleculeByMakeover.Domain.Entities;

namespace MoleculeByMakeover.Infrastructure.Persistence.Configurations;

public class GalleryImageConfiguration : IEntityTypeConfiguration<GalleryImage>
{
    public void Configure(EntityTypeBuilder<GalleryImage> builder)
    {
        builder.ToTable("GalleryImages");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Url).IsRequired();
        builder.Property(x => x.FileName).HasMaxLength(300).IsRequired();
        builder.Property(x => x.MimeType).HasMaxLength(100).IsRequired();

        builder.HasOne(x => x.UploadedByUser).WithMany()
            .HasForeignKey(x => x.UploadedByUserId).OnDelete(DeleteBehavior.SetNull);
    }
}

public class SettingConfiguration : IEntityTypeConfiguration<Setting>
{
    public void Configure(EntityTypeBuilder<Setting> builder)
    {
        builder.ToTable("Settings");
        builder.HasKey(x => x.Key);
        builder.Property(x => x.Key).HasMaxLength(150);
        builder.Property(x => x.Group).HasMaxLength(50).IsRequired();
    }
}

public class ProcessedPaymentEventConfiguration : IEntityTypeConfiguration<ProcessedPaymentEvent>
{
    public void Configure(EntityTypeBuilder<ProcessedPaymentEvent> builder)
    {
        builder.ToTable("ProcessedPaymentEvents");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ProviderKey).HasMaxLength(50).IsRequired();
        builder.Property(x => x.EventId).HasMaxLength(200).IsRequired();
        builder.HasIndex(x => new { x.ProviderKey, x.EventId }).IsUnique();
    }
}
