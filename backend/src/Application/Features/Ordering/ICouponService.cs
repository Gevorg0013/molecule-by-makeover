namespace MoleculeByMakeover.Application.Features.Ordering;

public interface ICouponService
{
    Task<List<CouponDto>> GetAllAsync(CancellationToken ct = default);
    Task<CouponDto> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Guid> CreateAsync(CouponUpsertRequest request, CancellationToken ct = default);
    Task UpdateAsync(Guid id, CouponUpsertRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
