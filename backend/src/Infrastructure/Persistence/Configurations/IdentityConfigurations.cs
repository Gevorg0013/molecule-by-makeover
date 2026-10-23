using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MoleculeByMakeover.Domain.Entities;

namespace MoleculeByMakeover.Infrastructure.Persistence.Configurations;

public class LanguageConfiguration : IEntityTypeConfiguration<Language>
{
    public void Configure(EntityTypeBuilder<Language> builder)
    {
        builder.ToTable("Languages");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(5).IsRequired();
        builder.HasIndex(x => x.Code).IsUnique();
        builder.Property(x => x.Name).HasMaxLength(50).IsRequired();
    }
}

// No foreign key to Users: the trail must outlive account changes and must also record attempts
// against emails that match no account.
public class AuthenticationLogConfiguration : IEntityTypeConfiguration<AuthenticationLog>
{
    public void Configure(EntityTypeBuilder<AuthenticationLog> builder)
    {
        builder.ToTable("AuthenticationLogs");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Email).HasMaxLength(256).IsRequired();
        builder.Property(x => x.IpAddress).HasMaxLength(45);
        builder.Property(x => x.UserAgent).HasMaxLength(512);

        // Lookups are "recent attempts for this account / email / address", newest first.
        builder.HasIndex(x => x.OccurredAt);
        builder.HasIndex(x => new { x.UserId, x.OccurredAt });
        builder.HasIndex(x => new { x.Email, x.OccurredAt });
        builder.HasIndex(x => new { x.IpAddress, x.OccurredAt });
    }
}

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(50).IsRequired();
        builder.HasIndex(x => x.Name).IsUnique();
    }
}

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Email).HasMaxLength(256).IsRequired();
        builder.HasIndex(x => x.Email).IsUnique();
        builder.Property(x => x.PasswordHash).IsRequired();
        builder.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.LastName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.PhoneNumber).HasMaxLength(30);

        builder.HasOne(x => x.PreferredLanguage).WithMany()
            .HasForeignKey(x => x.PreferredLanguageId).OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.Wishlist).WithOne(w => w.User)
            .HasForeignKey<Wishlist>(w => w.UserId).OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Cart).WithOne(c => c.User)
            .HasForeignKey<Cart>(c => c.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("UserRoles");
        builder.HasKey(x => new { x.UserId, x.RoleId });

        builder.HasOne(x => x.User).WithMany(u => u.UserRoles)
            .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Role).WithMany(r => r.UserRoles)
            .HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TokenHash).IsRequired();
        builder.HasIndex(x => x.TokenHash).IsUnique();

        builder.HasOne(x => x.User).WithMany(u => u.RefreshTokens)
            .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class PasswordResetTokenConfiguration : IEntityTypeConfiguration<PasswordResetToken>
{
    public void Configure(EntityTypeBuilder<PasswordResetToken> builder)
    {
        builder.ToTable("PasswordResetTokens");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TokenHash).IsRequired();
        builder.HasIndex(x => x.TokenHash).IsUnique();

        builder.HasOne(x => x.User).WithMany(u => u.PasswordResetTokens)
            .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}
