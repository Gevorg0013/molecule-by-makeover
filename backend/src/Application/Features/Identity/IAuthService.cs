namespace MoleculeByMakeover.Application.Features.Identity;

public interface IAuthService
{
    Task<AuthResult> RegisterAsync(RegisterRequest request, string? ipAddress, CancellationToken ct = default);
    Task<AuthResult> LoginAsync(LoginRequest request, string? ipAddress, CancellationToken ct = default);
    Task<AuthResult> RefreshAsync(string rawRefreshToken, string? ipAddress, CancellationToken ct = default);
    Task LogoutAsync(string rawRefreshToken, CancellationToken ct = default);
}
