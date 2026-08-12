namespace MoleculeByMakeover.Application.Features.Identity;

public record RegisterRequest(string Email, string Password, string FirstName, string LastName, string? PhoneNumber, string? PreferredLanguageCode);

public record LoginRequest(string Email, string Password);

public record RefreshRequest(string RefreshToken);

public record ForgotPasswordRequest(string Email);

public record ResetPasswordRequest(string Email, string Token, string NewPassword);

public record AuthResult(string AccessToken, string RefreshToken, DateTimeOffset AccessTokenExpiresAt, UserProfileDto User);

public record UserProfileDto(Guid Id, string Email, string FirstName, string LastName, string? PhoneNumber, IReadOnlyList<string> Roles);
