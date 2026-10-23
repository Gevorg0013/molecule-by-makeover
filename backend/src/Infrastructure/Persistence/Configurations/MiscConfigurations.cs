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

public class AppLogConfiguration : IEntityTypeConfiguration<AppLog>
{
    public void Configure(EntityTypeBuilder<AppLog> builder)
    {
        // Written by the API's DatabaseLogSink with raw SQL; keep column names in step with it.
        builder.ToTable("AppLogs");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityAlwaysColumn();
        builder.Property(x => x.Level).HasMaxLength(16).IsRequired();
        builder.Property(x => x.Message).IsRequired();
        builder.Property(x => x.MessageTemplate).IsRequired();
        builder.Property(x => x.SourceContext).HasMaxLength(512);
        builder.Property(x => x.RequestPath).HasMaxLength(2048);
        builder.Property(x => x.UserId).HasMaxLength(64);
        builder.Property(x => x.Properties).HasColumnType("jsonb");

        builder.HasIndex(x => x.Timestamp);
        builder.HasIndex(x => new { x.Level, x.Timestamp });
        builder.HasIndex(x => new { x.UserId, x.Timestamp });
    }
}

public class DataChangeLogConfiguration : IEntityTypeConfiguration<DataChangeLog>
{
    public void Configure(EntityTypeBuilder<DataChangeLog> builder)
    {
        // Written by the log_data_change() trigger; keep column names in step with it.
        builder.ToTable("DataChangeLogs");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityAlwaysColumn();
        builder.Property(x => x.TableName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Operation).HasMaxLength(10).IsRequired();
        builder.Property(x => x.RowId).HasMaxLength(200);
        builder.Property(x => x.OldData).HasColumnType("jsonb");
        builder.Property(x => x.NewData).HasColumnType("jsonb");
        builder.Property(x => x.AppUserId).HasMaxLength(64);
        builder.Property(x => x.DbUser).HasMaxLength(100).IsRequired();
        builder.Property(x => x.ChangedAt).HasDefaultValueSql("clock_timestamp()");

        builder.HasIndex(x => x.ChangedAt);
        builder.HasIndex(x => new { x.TableName, x.RowId, x.ChangedAt });
        builder.HasIndex(x => new { x.AppUserId, x.ChangedAt });
        builder.HasIndex(x => x.TransactionId);
    }
}
