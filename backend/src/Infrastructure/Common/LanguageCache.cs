using System.Collections.Concurrent;
using MoleculeByMakeover.Domain.Interfaces;
using MoleculeByMakeover.Shared.Constants;

namespace MoleculeByMakeover.Infrastructure.Common;

/// <summary>
/// Singleton, in-memory Code -&gt; LanguageId lookup. The Languages table is tiny and
/// changes only through Admin actions, so we load it once at startup (see Program.cs)
/// and refresh on demand, rather than hitting Postgres on every request just to resolve
/// the active culture into a foreign key.
/// </summary>
public class LanguageCache
{
    private readonly ConcurrentDictionary<string, int> _codeToId = new();
    public int DefaultLanguageId { get; private set; }
    public string DefaultLanguageCode { get; private set; } = LanguageCodes.Default;

    public async Task LoadAsync(IUnitOfWork unitOfWork, CancellationToken ct = default)
    {
        var languages = unitOfWork.Languages.Query().ToList();
        _codeToId.Clear();
        foreach (var language in languages)
            _codeToId[language.Code] = language.Id;

        var defaultLanguage = languages.FirstOrDefault(l => l.IsDefault) ?? languages.FirstOrDefault();
        if (defaultLanguage is not null)
        {
            DefaultLanguageId = defaultLanguage.Id;
            DefaultLanguageCode = defaultLanguage.Code;
        }

        await Task.CompletedTask;
    }

    public bool TryGetId(string code, out int id) => _codeToId.TryGetValue(code, out id);
}
