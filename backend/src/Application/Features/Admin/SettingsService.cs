using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MoleculeByMakeover.Domain.Entities;
using MoleculeByMakeover.Domain.Interfaces;
using MoleculeByMakeover.Shared.Exceptions;

namespace MoleculeByMakeover.Application.Features.Admin;

public class SettingsService(IUnitOfWork unitOfWork, IMapper mapper) : ISettingsService
{
    public async Task<List<SettingDto>> GetByGroupAsync(string group, CancellationToken ct = default)
    {
        var settings = await unitOfWork.Settings.GetByGroupAsync(group, ct);
        return mapper.Map<List<SettingDto>>(settings);
    }

    public async Task SetAsync(string key, string group, UpdateSettingRequest request, CancellationToken ct = default)
    {
        await unitOfWork.Settings.UpsertAsync(key, request.Value, group, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}

public class LanguageService(IUnitOfWork unitOfWork, IMapper mapper) : ILanguageService
{
    public async Task<List<LanguageDto>> GetAllAsync(CancellationToken ct = default)
    {
        var languages = await unitOfWork.Languages.Query().OrderBy(l => l.Id).ToListAsync(ct);
        return mapper.Map<List<LanguageDto>>(languages);
    }

    public async Task<int> CreateAsync(LanguageUpsertRequest request, CancellationToken ct = default)
    {
        if (await unitOfWork.Languages.GetByCodeAsync(request.Code, ct) is not null)
            throw new ConflictException($"Language '{request.Code}' already exists.");

        var language = new Language { Code = request.Code, Name = request.Name, IsDefault = request.IsDefault, IsActive = request.IsActive };
        await unitOfWork.Languages.AddAsync(language, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return language.Id;
    }

    public async Task UpdateAsync(int id, LanguageUpsertRequest request, CancellationToken ct = default)
    {
        var language = await unitOfWork.Languages.Query().FirstOrDefaultAsync(l => l.Id == id, ct)
            ?? throw new NotFoundException(nameof(Language), id);

        language.Code = request.Code;
        language.Name = request.Name;
        language.IsDefault = request.IsDefault;
        language.IsActive = request.IsActive;

        unitOfWork.Languages.Update(language);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
