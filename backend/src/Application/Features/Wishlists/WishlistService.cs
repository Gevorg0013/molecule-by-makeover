using Microsoft.EntityFrameworkCore;
using MoleculeByMakeover.Application.Common.Interfaces;
using MoleculeByMakeover.Domain.Entities;
using MoleculeByMakeover.Domain.Interfaces;
using MoleculeByMakeover.Shared.Exceptions;

namespace MoleculeByMakeover.Application.Features.Wishlists;

public class WishlistService(IUnitOfWork unitOfWork, ICurrentLanguageService currentLanguage) : IWishlistService
{
    public async Task<WishlistDto> GetAsync(Guid userId, CancellationToken ct = default)
    {
        var wishlist = await unitOfWork.Wishlists.Query()
            .Include(w => w.Items).ThenInclude(i => i.Product).ThenInclude(p => p.Translations)
            .FirstOrDefaultAsync(w => w.UserId == userId, ct);

        if (wishlist is null) return new WishlistDto([]);

        var items = wishlist.Items.Select(i =>
        {
            var translation = i.Product.Translations.FirstOrDefault(t => t.LanguageId == currentLanguage.LanguageId)
                ?? i.Product.Translations.FirstOrDefault();
            return new WishlistItemDto(
                i.ProductId, translation?.Name ?? string.Empty, translation?.Slug ?? string.Empty,
                i.Product.MainImageUrl, i.Product.Price, i.Product.FinalPrice, i.Product.Stock > 0, i.AddedAt);
        }).ToList();

        return new WishlistDto(items);
    }

    public async Task AddAsync(Guid userId, Guid productId, CancellationToken ct = default)
    {
        var product = await unitOfWork.Products.GetByIdAsync(productId, ct) ?? throw new NotFoundException(nameof(Product), productId);

        var wishlist = await unitOfWork.Wishlists.GetByUserIdAsync(userId, ct);
        if (wishlist is null)
        {
            wishlist = new Wishlist { UserId = userId, CreatedAt = DateTimeOffset.UtcNow };
            await unitOfWork.Wishlists.AddAsync(wishlist, ct);
            await unitOfWork.SaveChangesAsync(ct);
        }

        if (wishlist.Items.Any(i => i.ProductId == productId)) return;

        var newItem = new WishlistItem { WishlistId = wishlist.Id, ProductId = product.Id, AddedAt = DateTimeOffset.UtcNow };
        wishlist.Items.Add(newItem);
        await unitOfWork.Wishlists.AddItemAsync(newItem, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task RemoveAsync(Guid userId, Guid productId, CancellationToken ct = default)
    {
        var wishlist = await unitOfWork.Wishlists.GetByUserIdAsync(userId, ct);
        var item = wishlist?.Items.FirstOrDefault(i => i.ProductId == productId);
        if (wishlist is null || item is null) return;

        wishlist.Items.Remove(item);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
