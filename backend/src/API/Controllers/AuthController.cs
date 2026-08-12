using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using MoleculeByMakeover.API.Common;
using MoleculeByMakeover.Application.Features.Identity;
using MoleculeByMakeover.Application.Features.Ordering;
using MoleculeByMakeover.Shared.Constants;

namespace MoleculeByMakeover.API.Controllers;

[EnableRateLimiting("auth")]
public class AuthController(IAuthService authService, ICartService cartService) : ApiControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<UserProfileDto>> Register(RegisterRequest request, CancellationToken ct)
    {
        var result = await authService.RegisterAsync(request, HttpContext.Connection.RemoteIpAddress?.ToString(), ct);
        Response.SetRefreshTokenCookie(result.RefreshToken, DateTimeOffset.UtcNow.AddDays(14));
        return Ok(new { accessToken = result.AccessToken, expiresAt = result.AccessTokenExpiresAt, user = result.User });
    }

    [HttpPost("login")]
    public async Task<ActionResult> Login(LoginRequest request, CancellationToken ct)
    {
        var result = await authService.LoginAsync(request, HttpContext.Connection.RemoteIpAddress?.ToString(), ct);
        Response.SetRefreshTokenCookie(result.RefreshToken, DateTimeOffset.UtcNow.AddDays(14));

        if (Request.Cookies.TryGetValue("guestToken", out var guestToken) && !string.IsNullOrWhiteSpace(guestToken))
        {
            await cartService.MergeGuestCartAsync(guestToken, result.User.Id, ct);
            Response.ClearGuestTokenCookie();
        }

        return Ok(new { accessToken = result.AccessToken, expiresAt = result.AccessTokenExpiresAt, user = result.User });
    }

    [HttpPost("refresh")]
    public async Task<ActionResult> Refresh(CancellationToken ct)
    {
        var refreshToken = Request.GetRefreshToken();
        if (string.IsNullOrWhiteSpace(refreshToken))
            return Unauthorized();

        var result = await authService.RefreshAsync(refreshToken, HttpContext.Connection.RemoteIpAddress?.ToString(), ct);
        Response.SetRefreshTokenCookie(result.RefreshToken, DateTimeOffset.UtcNow.AddDays(14));
        return Ok(new { accessToken = result.AccessToken, expiresAt = result.AccessTokenExpiresAt, user = result.User });
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        var refreshToken = Request.GetRefreshToken();
        if (!string.IsNullOrWhiteSpace(refreshToken))
            await authService.LogoutAsync(refreshToken, ct);

        Response.ClearRefreshTokenCookie();
        return NoContent();
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request, CancellationToken ct)
    {
        await authService.ForgotPasswordAsync(request, ct);
        return NoContent();
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest request, CancellationToken ct)
    {
        await authService.ResetPasswordAsync(request, ct);
        return NoContent();
    }

    [HttpGet("me")]
    [Authorize]
    public ActionResult Me() => Ok(new
    {
        userId = CurrentUser.UserId,
        email = CurrentUser.Email,
        isAdmin = CurrentUser.IsInRole(RoleNames.Admin)
    });
}
