using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MoleculeByMakeover.Domain.Entities;
using MoleculeByMakeover.Domain.Interfaces;
using MoleculeByMakeover.Shared.Exceptions;

namespace MoleculeByMakeover.Application.Features.Ordering;

public class CouponService(IUnitOfWork unitOfWork, IMapper mapper, IValidator<CouponUpsertRequest> validator) : ICouponService
{
    public async Task<List<CouponDto>> GetAllAsync(CancellationToken ct = default)
    {
        var coupons = await unitOfWork.Coupons.Query().OrderByDescending(c => c.CreatedAt).ToListAsync(ct);
        return mapper.Map<List<CouponDto>>(coupons);
    }

    public async Task<CouponDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var coupon = await unitOfWork.Coupons.GetByIdAsync(id, ct) ?? throw new NotFoundException(nameof(Coupon), id);
        return mapper.Map<CouponDto>(coupon);
    }

    public async Task<Guid> CreateAsync(CouponUpsertRequest request, CancellationToken ct = default)
    {
        await validator.ValidateAndThrowAsync(request, ct);

        var code = request.Code.Trim().ToUpperInvariant();
        if (await unitOfWork.Coupons.GetByCodeAsync(code, ct) is not null)
            throw new ConflictException($"A coupon with code '{code}' already exists.");

        var coupon = new Coupon
        {
            Code = code,
            DiscountType = request.DiscountType,
            DiscountValue = request.DiscountValue,
            MinOrderAmount = request.MinOrderAmount,
            MaxUses = request.MaxUses,
            StartsAt = request.StartsAt,
            ExpiresAt = request.ExpiresAt,
            IsActive = request.IsActive,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await unitOfWork.Coupons.AddAsync(coupon, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return coupon.Id;
    }

    public async Task UpdateAsync(Guid id, CouponUpsertRequest request, CancellationToken ct = default)
    {
        await validator.ValidateAndThrowAsync(request, ct);

        var coupon = await unitOfWork.Coupons.GetByIdAsync(id, ct) ?? throw new NotFoundException(nameof(Coupon), id);

        coupon.Code = request.Code.Trim().ToUpperInvariant();
        coupon.DiscountType = request.DiscountType;
        coupon.DiscountValue = request.DiscountValue;
        coupon.MinOrderAmount = request.MinOrderAmount;
        coupon.MaxUses = request.MaxUses;
        coupon.StartsAt = request.StartsAt;
        coupon.ExpiresAt = request.ExpiresAt;
        coupon.IsActive = request.IsActive;
        coupon.UpdatedAt = DateTimeOffset.UtcNow;

        unitOfWork.Coupons.Update(coupon);
        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var coupon = await unitOfWork.Coupons.GetByIdAsync(id, ct) ?? throw new NotFoundException(nameof(Coupon), id);
        unitOfWork.Coupons.Remove(coupon);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
