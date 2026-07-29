using MoleculeByMakeover.Infrastructure.Common;
using MoleculeByMakeover.Shared.Constants;

namespace MoleculeByMakeover.API.Middleware;

// Resolves the active language ONCE per request - ?lang= query param, then Accept-Language
// header, then the tenant default - and stashes the code in HttpContext.Items so
// CurrentLanguageService (Infrastructure) can read it synchronously without another lookup.
public class LocalizationMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, LanguageCache languageCache)
    {
        var code = ResolveLanguageCode(context, languageCache);
        context.Items[HttpContextItemKeys.LanguageCode] = code;

        await next(context);
    }

    private static string ResolveLanguageCode(HttpContext context, LanguageCache languageCache)
    {
        if (context.Request.Query.TryGetValue("lang", out var queryLang) &&
            LanguageCodes.All.Contains(queryLang.ToString()))
            return queryLang.ToString();

        var acceptLanguage = context.Request.Headers.AcceptLanguage.ToString();
        if (!string.IsNullOrWhiteSpace(acceptLanguage))
        {
            var preferred = acceptLanguage.Split(',')
                .Select(part => part.Split(';')[0].Trim())
                .FirstOrDefault(candidate => LanguageCodes.All.Contains(candidate));

            if (preferred is not null) return preferred;
        }

        return languageCache.DefaultLanguageCode;
    }
}
