using MoleculeByMakeover.Domain.Enums;

namespace MoleculeByMakeover.Application.Features.Ordering;

public record CouponDto(
    Guid Id,
    string Code,
    DiscountType DiscountType,
    decimal DiscountValue,
    decimal? MinOrderAmount,
    int? MaxUses,
    int UsesCount,
    DateTimeOffset? StartsAt,
    DateTimeOffset? ExpiresAt,
    bool IsActive);

public record CouponUpsertRequest(
    string Code,
    DiscountType DiscountType,
    decimal DiscountValue,
    decimal? MinOrderAmount,
    int? MaxUses,
    DateTimeOffset? StartsAt,
    DateTimeOffset? ExpiresAt,
    bool IsActive);
