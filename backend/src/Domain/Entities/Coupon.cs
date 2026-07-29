using MoleculeByMakeover.Domain.Common;
using MoleculeByMakeover.Domain.Enums;

namespace MoleculeByMakeover.Domain.Entities;

public class Coupon : BaseEntity
{
    public string Code { get; set; } = default!;
    public DiscountType DiscountType { get; set; } = DiscountType.Percent;
    public decimal DiscountValue { get; set; }
    public decimal? MinOrderAmount { get; set; }
    public int? MaxUses { get; set; }
    public int UsesCount { get; set; }
    public DateTimeOffset? StartsAt { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;

    public bool IsValidNow()
    {
        var now = DateTimeOffset.UtcNow;
        if (!IsActive) return false;
        if (StartsAt is not null && now < StartsAt) return false;
        if (ExpiresAt is not null && now > ExpiresAt) return false;
        if (MaxUses is not null && UsesCount >= MaxUses) return false;
        return true;
    }
}
