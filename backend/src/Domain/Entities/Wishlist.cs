using MoleculeByMakeover.Domain.Common;

namespace MoleculeByMakeover.Domain.Entities;

public class Wishlist : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = default!;

    public ICollection<WishlistItem> Items { get; set; } = [];
}

public class WishlistItem
{
    public Guid WishlistId { get; set; }
    public Wishlist Wishlist { get; set; } = default!;

    public Guid ProductId { get; set; }
    public Product Product { get; set; } = default!;

    public DateTimeOffset AddedAt { get; set; } = DateTimeOffset.UtcNow;
}
