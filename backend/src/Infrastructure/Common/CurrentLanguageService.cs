using Microsoft.AspNetCore.Http;
using MoleculeByMakeover.Application.Common.Interfaces;

namespace MoleculeByMakeover.Infrastructure.Common;

// The resolved language CODE is set once per request by the API's LocalizationMiddleware
// (query param -> Accept-Language header -> default). This service just maps that code
// to its numeric LanguageId via the in-memory LanguageCache - no per-request DB call.
public class CurrentLanguageService(IHttpContextAccessor httpContextAccessor, LanguageCache languageCache) : ICurrentLanguageService
{
    public string LanguageCode =>
        httpContextAccessor.HttpContext?.Items[HttpContextItemKeys.LanguageCode] as string ?? languageCache.DefaultLanguageCode;

    public int LanguageId => languageCache.TryGetId(LanguageCode, out var id) ? id : languageCache.DefaultLanguageId;
}

public static class HttpContextItemKeys
{
    public const string LanguageCode = "CurrentLanguageCode";
}
