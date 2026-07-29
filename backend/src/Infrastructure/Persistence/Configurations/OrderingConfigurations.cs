using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MoleculeByMakeover.Domain.Entities;

namespace MoleculeByMakeover.Infrastructure.Persistence.Configurations;

public class WishlistConfiguration : IEntityTypeConfiguration<Wishlist>
{
    public void Configure(EntityTypeBuilder<Wishlist> builder)
    {
        builder.ToTable("Wishlists");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.UserId).IsUnique();
    }
}

public class WishlistItemConfiguration : IEntityTypeConfiguration<WishlistItem>
{
    public void Configure(EntityTypeBuilder<WishlistItem> builder)
    {
        builder.ToTable("WishlistItems");
        builder.HasKey(x => new { x.WishlistId, x.ProductId });
        builder.HasQueryFilter(x => !x.Product.IsDeleted);

        builder.HasOne(x => x.Wishlist).WithMany(w => w.Items)
            .HasForeignKey(x => x.WishlistId).OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Product).WithMany()
            .HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class CartConfiguration : IEntityTypeConfiguration<Cart>
{
    public void Configure(EntityTypeBuilder<Cart> builder)
    {
        builder.ToTable("Carts");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.UserId).IsUnique();
        builder.HasIndex(x => x.GuestToken).IsUnique();

        builder.HasOne(x => x.Coupon).WithMany()
            .HasForeignKey(x => x.CouponId).OnDelete(DeleteBehavior.SetNull);
    }
}

public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.ToTable("CartItems");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.UnitPriceSnapshot).HasColumnType("numeric(18,2)");
        builder.HasIndex(x => new { x.CartId, x.ProductId }).IsUnique();
        builder.HasQueryFilter(x => !x.Product.IsDeleted);

        builder.HasOne(x => x.Cart).WithMany(c => c.Items)
            .HasForeignKey(x => x.CartId).OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Product).WithMany()
            .HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class CouponConfiguration : IEntityTypeConfiguration<Coupon>
{
    public void Configure(EntityTypeBuilder<Coupon> builder)
    {
        builder.ToTable("Coupons");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.HasIndex(x => x.Code).IsUnique();
        builder.Property(x => x.DiscountValue).HasColumnType("numeric(18,2)");
        builder.Property(x => x.MinOrderAmount).HasColumnType("numeric(18,2)");
    }
}

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.OrderNumber).HasMaxLength(50).IsRequired();
        builder.HasIndex(x => x.OrderNumber).IsUnique();
        builder.HasIndex(x => x.PaymentReference);

        builder.Property(x => x.SubTotal).HasColumnType("numeric(18,2)");
        builder.Property(x => x.DiscountTotal).HasColumnType("numeric(18,2)");
        builder.Property(x => x.ShippingTotal).HasColumnType("numeric(18,2)");
        builder.Property(x => x.GrandTotal).HasColumnType("numeric(18,2)");
        builder.Property(x => x.Currency).HasMaxLength(3).IsRequired();
        builder.Property(x => x.PaymentProvider).HasMaxLength(50).IsRequired();

        builder.OwnsOne(x => x.ShippingAddress, a =>
        {
            a.Property(p => p.FullName).HasMaxLength(200).IsRequired();
            a.Property(p => p.Phone).HasMaxLength(30).IsRequired();
            a.Property(p => p.Country).HasMaxLength(100).IsRequired();
            a.Property(p => p.City).HasMaxLength(100).IsRequired();
            a.Property(p => p.AddressLine1).HasMaxLength(300).IsRequired();
            a.Property(p => p.AddressLine2).HasMaxLength(300);
            a.Property(p => p.PostalCode).HasMaxLength(20);
        });
        builder.Navigation(x => x.ShippingAddress).IsRequired();

        builder.HasOne(x => x.User).WithMany(u => u.Orders)
            .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Coupon).WithMany()
            .HasForeignKey(x => x.CouponId).OnDelete(DeleteBehavior.SetNull);
    }
}

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ProductNameSnapshot).HasMaxLength(300).IsRequired();
        builder.Property(x => x.SkuSnapshot).HasMaxLength(64).IsRequired();
        builder.Property(x => x.UnitPriceSnapshot).HasColumnType("numeric(18,2)");
        builder.Property(x => x.LineTotal).HasColumnType("numeric(18,2)");

        builder.HasOne(x => x.Order).WithMany(o => o.Items)
            .HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Product).WithMany()
            .HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.SetNull);
    }
}

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.ToTable("Reviews");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.ProductId, x.UserId }).IsUnique();
        builder.Property(x => x.Comment).HasMaxLength(2000);
        builder.HasQueryFilter(x => !x.Product.IsDeleted);

        builder.HasOne(x => x.Product).WithMany(p => p.Reviews)
            .HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.User).WithMany(u => u.Reviews)
            .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}
