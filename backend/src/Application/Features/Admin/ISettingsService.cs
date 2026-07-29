namespace MoleculeByMakeover.Application.Features.Admin;

public interface ISettingsService
{
    Task<List<SettingDto>> GetByGroupAsync(string group, CancellationToken ct = default);
    Task SetAsync(string key, string group, UpdateSettingRequest request, CancellationToken ct = default);
}

public interface ILanguageService
{
    Task<List<LanguageDto>> GetAllAsync(CancellationToken ct = default);
    Task<int> CreateAsync(LanguageUpsertRequest request, CancellationToken ct = default);
    Task UpdateAsync(int id, LanguageUpsertRequest request, CancellationToken ct = default);
}
