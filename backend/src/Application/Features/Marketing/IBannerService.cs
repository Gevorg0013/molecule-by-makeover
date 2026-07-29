namespace MoleculeByMakeover.Application.Features.Marketing;

public interface IBannerService
{
    Task<List<BannerDto>> GetActiveAsync(CancellationToken ct = default);

    Task<List<BannerAdminDto>> GetAllAdminAsync(CancellationToken ct = default);
    Task<Guid> CreateAsync(BannerUpsertRequest request, CancellationToken ct = default);
    Task UpdateAsync(Guid id, BannerUpsertRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}

public interface INewsletterService
{
    Task SubscribeAsync(NewsletterSubscribeRequest request, CancellationToken ct = default);
}
