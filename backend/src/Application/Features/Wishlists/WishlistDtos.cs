namespace MoleculeByMakeover.Application.Features.Wishlists;

public record WishlistItemDto(Guid ProductId, string Name, string Slug, string? ImageUrl, decimal Price, decimal FinalPrice, bool InStock, DateTimeOffset AddedAt);

public record WishlistDto(List<WishlistItemDto> Items);
