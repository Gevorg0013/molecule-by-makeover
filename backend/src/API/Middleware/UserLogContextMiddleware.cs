using System.Security.Claims;
using Serilog.Context;

namespace MoleculeByMakeover.API.Middleware;

// Pushes the authenticated user's id onto the Serilog LogContext so every log written while
// handling the request (order placed, product updated, ...) records who triggered it. Runs after
// UseAuthentication; anonymous requests are logged without a UserId.
public class UserLogContextMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? context.User.FindFirstValue("sub");
        if (userId is null)
        {
            await next(context);
            return;
        }

        using (LogContext.PushProperty("UserId", userId))
            await next(context);
    }
}
