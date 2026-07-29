using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MoleculeByMakeover.Application.Common.Interfaces;
using MoleculeByMakeover.Domain.Entities;
using MoleculeByMakeover.Domain.Interfaces;
using MoleculeByMakeover.Shared.Constants;
using MoleculeByMakeover.Shared.Exceptions;

namespace MoleculeByMakeover.Application.Features.Ordering;

public class CartService(
    IUnitOfWork unitOfWork,
    ICurrentLanguageService currentLanguage,
    IValidator<AddCartItemRequest> addItemValidator,
    IValidator<UpdateCartItemRequest> updateItemValidator) : ICartService
{
    public async Task<CartDto> GetCartAsync(CartContext context, CancellationToken ct = default)
    {
        var cart = await TryFindCartAsync(context, ct);
        return cart is null ? EmptyCart() : ToDto(cart);
    }

    public async Task<CartDto> AddItemAsync(CartContext context, AddCartItemRequest request, CancellationToken ct = default)
    {
        await addItemValidator.ValidateAndThrowAsync(request, ct);

        var product = await unitOfWork.Products.GetByIdAsync(request.ProductId, ct)
            ?? throw new NotFoundException(nameof(Product), request.ProductId);

        if (!product.IsActive || product.IsDeleted)
            throw new BadRequestException("This product is not available.");

        var cart = await GetOrCreateCartAsync(context, ct);

        var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == request.ProductId);
        var desiredQuantity = (existingItem?.Quantity ?? 0) + request.Quantity;
        if (desiredQuantity > product.Stock)
            throw new BadRequestException($"Only {product.Stock} unit(s) of this product are in stock.");

        if (existingItem is not null)
        {
            existingItem.Quantity = desiredQuantity;
        }
        else
        {
            var newItem = new CartItem
            {
                CartId = cart.Id,
                ProductId = product.Id,
                Quantity = request.Quantity,
                UnitPriceSnapshot = product.FinalPrice,
                CreatedAt = DateTimeOffset.UtcNow
            };
            cart.Items.Add(newItem);
            await unitOfWork.Carts.AddItemAsync(newItem, ct);
        }

        await unitOfWork.SaveChangesAsync(ct);

        return await ReloadAndMapAsync(cart.Id, ct);
    }

    public async Task<CartDto> UpdateItemAsync(CartContext context, Guid itemId, UpdateCartItemRequest request, CancellationToken ct = default)
    {
        await updateItemValidator.ValidateAndThrowAsync(request, ct);

        var cart = await TryFindCartAsync(context, ct) ?? throw new NotFoundException(nameof(Cart), "current");
        var item = cart.Items.FirstOrDefault(i => i.Id == itemId) ?? throw new NotFoundException(nameof(CartItem), itemId);

        if (request.Quantity > item.Product.Stock)
            throw new BadRequestException($"Only {item.Product.Stock} unit(s) of this product are in stock.");

        item.Quantity = request.Quantity;
        await unitOfWork.SaveChangesAsync(ct);

        return await ReloadAndMapAsync(cart.Id, ct);
    }

    public async Task<CartDto> RemoveItemAsync(CartContext context, Guid itemId, CancellationToken ct = default)
    {
        var cart = await TryFindCartAsync(context, ct) ?? throw new NotFoundException(nameof(Cart), "current");
        var item = cart.Items.FirstOrDefault(i => i.Id == itemId) ?? throw new NotFoundException(nameof(CartItem), itemId);

        cart.Items.Remove(item);
        await unitOfWork.SaveChangesAsync(ct);

        return await ReloadAndMapAsync(cart.Id, ct);
    }

    public async Task<CartDto> ApplyCouponAsync(CartContext context, ApplyCouponRequest request, CancellationToken ct = default)
    {
        var cart = await TryFindCartAsync(context, ct) ?? throw new NotFoundException(nameof(Cart), "current");

        var coupon = await unitOfWork.Coupons.GetByCodeAsync(request.Code.Trim().ToUpperInvariant(), ct);
        if (coupon is null || !coupon.IsValidNow())
            throw new BadRequestException("This coupon code is invalid or has expired.");

        var subTotal = cart.Items.Sum(i => i.UnitPriceSnapshot * i.Quantity);
        if (coupon.MinOrderAmount is not null && subTotal < coupon.MinOrderAmount)
            throw new BadRequestException($"This coupon requires a minimum order of {coupon.MinOrderAmount}.");

        cart.CouponId = coupon.Id;
        await unitOfWork.SaveChangesAsync(ct);

        return await ReloadAndMapAsync(cart.Id, ct);
    }

    public async Task<CartDto> RemoveCouponAsync(CartContext context, CancellationToken ct = default)
    {
        var cart = await TryFindCartAsync(context, ct) ?? throw new NotFoundException(nameof(Cart), "current");
        cart.CouponId = null;
        await unitOfWork.SaveChangesAsync(ct);

        return await ReloadAndMapAsync(cart.Id, ct);
    }

    public async Task MergeGuestCartAsync(string guestToken, Guid userId, CancellationToken ct = default)
    {
        var guestCart = await unitOfWork.Carts.GetActiveByGuestTokenAsync(guestToken, ct);
        if (guestCart is null || guestCart.Items.Count == 0) return;

        var userCart = await unitOfWork.Carts.GetActiveByUserIdAsync(userId, ct);
        if (userCart is null)
        {
            guestCart.UserId = userId;
            guestCart.GuestToken = null;
            await unitOfWork.SaveChangesAsync(ct);
            return;
        }

        foreach (var guestItem in guestCart.Items)
        {
            var existing = userCart.Items.FirstOrDefault(i => i.ProductId == guestItem.ProductId);
            if (existing is not null)
            {
                existing.Quantity += guestItem.Quantity;
            }
            else
            {
                var newItem = new CartItem
                {
                    CartId = userCart.Id,
                    ProductId = guestItem.ProductId,
                    Quantity = guestItem.Quantity,
                    UnitPriceSnapshot = guestItem.UnitPriceSnapshot,
                    CreatedAt = DateTimeOffset.UtcNow
                };
                userCart.Items.Add(newItem);
                await unitOfWork.Carts.AddItemAsync(newItem, ct);
            }
        }

        unitOfWork.Carts.Remove(guestCart);
        await unitOfWork.SaveChangesAsync(ct);
    }

    private async Task<Cart?> TryFindCartAsync(CartContext context, CancellationToken ct) =>
        context.UserId is { } userId
            ? await unitOfWork.Carts.GetActiveByUserIdAsync(userId, ct)
            : context.GuestToken is { } token
                ? await unitOfWork.Carts.GetActiveByGuestTokenAsync(token, ct)
                : null;

    private async Task<Cart> GetOrCreateCartAsync(CartContext context, CancellationToken ct)
    {
        var cart = await TryFindCartAsync(context, ct);
        if (cart is not null) return cart;

        cart = new Cart
        {
            UserId = context.UserId,
            GuestToken = context.UserId is null ? context.GuestToken ?? Guid.NewGuid().ToString("N") : null,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await unitOfWork.Carts.AddAsync(cart, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return cart;
    }

    private async Task<CartDto> ReloadAndMapAsync(Guid cartId, CancellationToken ct)
    {
        var cart = await unitOfWork.Carts.Query()
            .Include(c => c.Items).ThenInclude(i => i.Product).ThenInclude(p => p.Translations)
            .Include(c => c.Coupon)
            .FirstAsync(c => c.Id == cartId, ct);

        return ToDto(cart);
    }

    private CartDto ToDto(Cart cart)
    {
        var subTotal = cart.Items.Sum(i => i.UnitPriceSnapshot * i.Quantity);
        var discount = 0m;
        if (cart.Coupon is not null && cart.Coupon.IsValidNow())
        {
            discount = cart.Coupon.DiscountType == Domain.Enums.DiscountType.Percent
                ? Math.Round(subTotal * cart.Coupon.DiscountValue / 100m, 2)
                : Math.Min(subTotal, cart.Coupon.DiscountValue);
        }

        var items = cart.Items.Select(i =>
        {
            var translation = i.Product.Translations.FirstOrDefault(t => t.LanguageId == currentLanguage.LanguageId)
                ?? i.Product.Translations.FirstOrDefault();
            return new CartItemDto(
                i.Id, i.ProductId, translation?.Name ?? string.Empty, translation?.Slug ?? string.Empty,
                i.Product.MainImageUrl, i.UnitPriceSnapshot, i.Quantity, i.UnitPriceSnapshot * i.Quantity, i.Product.Stock);
        }).ToList();

        return new CartDto(cart.Id, items, subTotal, discount, subTotal - discount, cart.Coupon?.Code, StoreDefaults.Currency);
    }

    private static CartDto EmptyCart() => new(Guid.Empty, [], 0, 0, 0, null, StoreDefaults.Currency);
}
