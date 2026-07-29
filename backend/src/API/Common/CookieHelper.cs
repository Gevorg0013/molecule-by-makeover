namespace MoleculeByMakeover.API.Common;

public static class CookieHelper
{
    private const string RefreshTokenCookieName = "refreshToken";
    private const string GuestTokenCookieName = "guestToken";

    public static void SetRefreshTokenCookie(this HttpResponse response, string token, DateTimeOffset expiresAt)
    {
        response.Cookies.Append(RefreshTokenCookieName, token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = expiresAt,
            Path = "/api/v1/auth"
        });
    }

    public static void ClearRefreshTokenCookie(this HttpResponse response) =>
        response.Cookies.Delete(RefreshTokenCookieName, new CookieOptions { Path = "/api/v1/auth" });

    public static string? GetRefreshToken(this HttpRequest request) =>
        request.Cookies.TryGetValue(RefreshTokenCookieName, out var token) ? token : null;

    public static string GetOrCreateGuestToken(this HttpContext context)
    {
        if (context.Request.Cookies.TryGetValue(GuestTokenCookieName, out var existing) && !string.IsNullOrWhiteSpace(existing))
            return existing;

        var token = Guid.NewGuid().ToString("N");
        context.Response.Cookies.Append(GuestTokenCookieName, token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddDays(30)
        });
        return token;
    }

    public static void ClearGuestTokenCookie(this HttpResponse response) =>
        response.Cookies.Delete(GuestTokenCookieName);
}
