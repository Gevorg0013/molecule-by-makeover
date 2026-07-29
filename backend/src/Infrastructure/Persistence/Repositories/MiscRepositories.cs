using Microsoft.EntityFrameworkCore;
using MoleculeByMakeover.Domain.Entities;
using MoleculeByMakeover.Domain.Interfaces;

namespace MoleculeByMakeover.Infrastructure.Persistence.Repositories;

public class SettingRepository(AppDbContext context) : ISettingRepository
{
    public async Task<string?> GetValueAsync(string key, CancellationToken ct = default) =>
        (await context.Settings.FirstOrDefaultAsync(s => s.Key == key, ct))?.Value;

    public async Task<List<Setting>> GetByGroupAsync(string group, CancellationToken ct = default) =>
        await context.Settings.Where(s => s.Group == group).ToListAsync(ct);

    public async Task UpsertAsync(string key, string? value, string group, CancellationToken ct = default)
    {
        var setting = await context.Settings.FirstOrDefaultAsync(s => s.Key == key, ct);
        if (setting is null)
        {
            context.Settings.Add(new Setting { Key = key, Value = value, Group = group });
        }
        else
        {
            setting.Value = value;
            setting.Group = group;
        }
    }
}

public class LanguageRepository(AppDbContext context) : Repository<Language>(context), ILanguageRepository
{
    public async Task<Language?> GetByCodeAsync(string code, CancellationToken ct = default) =>
        await DbSet.FirstOrDefaultAsync(l => l.Code == code, ct);

    public async Task<Language> GetDefaultAsync(CancellationToken ct = default) =>
        await DbSet.FirstAsync(l => l.IsDefault, ct);
}

public class NewsletterSubscriberRepository(AppDbContext context) : Repository<NewsletterSubscriber>(context), INewsletterSubscriberRepository
{
    public async Task<bool> ExistsAsync(string email, CancellationToken ct = default) =>
        await DbSet.AnyAsync(s => s.Email == email, ct);
}

public class ProcessedPaymentEventRepository(AppDbContext context) : Repository<ProcessedPaymentEvent>(context), IProcessedPaymentEventRepository
{
    public async Task<bool> HasBeenProcessedAsync(string providerKey, string eventId, CancellationToken ct = default) =>
        await DbSet.AnyAsync(e => e.ProviderKey == providerKey && e.EventId == eventId, ct);
}
