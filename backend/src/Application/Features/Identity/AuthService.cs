using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MoleculeByMakeover.Application.Common.Interfaces;
using MoleculeByMakeover.Application.Common.Options;
using MoleculeByMakeover.Domain.Entities;
using MoleculeByMakeover.Domain.Interfaces;
using MoleculeByMakeover.Shared.Constants;
using MoleculeByMakeover.Shared.Exceptions;

namespace MoleculeByMakeover.Application.Features.Identity;

public class AuthService(
    IUnitOfWork unitOfWork,
    IJwtTokenService jwtTokenService,
    IPasswordHasherService passwordHasher,
    IValidator<RegisterRequest> registerValidator,
    IValidator<LoginRequest> loginValidator,
    IValidator<ForgotPasswordRequest> forgotPasswordValidator,
    IValidator<ResetPasswordRequest> resetPasswordValidator,
    IEmailSender emailSender,
    IConfiguration configuration,
    IOptions<JwtOptions> jwtOptions) : IAuthService
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;

    public async Task<AuthResult> RegisterAsync(RegisterRequest request, string? ipAddress, CancellationToken ct = default)
    {
        await registerValidator.ValidateAndThrowAsync(request, ct);

        var existing = await unitOfWork.Users.GetByEmailAsync(request.Email, ct);
        if (existing is not null)
            throw new ConflictException("An account with this email already exists.");

        int? preferredLanguageId = null;
        if (!string.IsNullOrWhiteSpace(request.PreferredLanguageCode))
        {
            var language = await unitOfWork.Languages.GetByCodeAsync(request.PreferredLanguageCode, ct);
            preferredLanguageId = language?.Id;
        }

        var user = new User
        {
            Email = request.Email.Trim().ToLowerInvariant(),
            PasswordHash = passwordHasher.Hash(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber,
            PreferredLanguageId = preferredLanguageId,
            CreatedAt = DateTimeOffset.UtcNow
        };

        var customerRole = await unitOfWork.Roles.GetByNameAsync(RoleNames.Customer, ct)
            ?? throw new InvalidOperationException($"Role '{RoleNames.Customer}' is not seeded.");

        user.UserRoles.Add(new UserRole { User = user, Role = customerRole });

        await unitOfWork.Users.AddAsync(user, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return await IssueTokensAsync(user, [RoleNames.Customer], ipAddress, ct);
    }

    public async Task<AuthResult> LoginAsync(LoginRequest request, string? ipAddress, CancellationToken ct = default)
    {
        await loginValidator.ValidateAndThrowAsync(request, ct);

        var user = await unitOfWork.Users.GetByEmailAsync(request.Email.Trim().ToLowerInvariant(), ct);
        if (user is null || !user.IsActive || !passwordHasher.Verify(user.PasswordHash, request.Password))
            throw new UnauthorizedException();

        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        return await IssueTokensAsync(user, roles, ipAddress, ct);
    }

    public async Task<AuthResult> RefreshAsync(string rawRefreshToken, string? ipAddress, CancellationToken ct = default)
    {
        var tokenHash = jwtTokenService.HashToken(rawRefreshToken);
        var existingToken = await unitOfWork.RefreshTokens.GetByTokenHashAsync(tokenHash, ct);

        if (existingToken is null)
            throw new UnauthorizedException("Invalid refresh token.");

        if (existingToken.RevokedAt is not null)
        {
            // Reuse of an already-rotated token: possible theft. Revoke the whole family.
            await RevokeAllActiveTokensForUserAsync(existingToken.UserId, ct);
            throw new UnauthorizedException("Refresh token has already been used. All sessions have been revoked.");
        }

        if (!existingToken.IsActive)
            throw new UnauthorizedException("Refresh token has expired.");

        var user = await unitOfWork.Users.GetWithRolesAsync(existingToken.UserId, ct)
            ?? throw new UnauthorizedException();

        var newRawToken = jwtTokenService.GenerateRefreshToken();
        var newTokenHash = jwtTokenService.HashToken(newRawToken);

        existingToken.RevokedAt = DateTimeOffset.UtcNow;
        existingToken.ReplacedByTokenHash = newTokenHash;
        unitOfWork.RefreshTokens.Update(existingToken);

        var newToken = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = newTokenHash,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(_jwtOptions.RefreshTokenDays),
            CreatedByIp = ipAddress,
            CreatedAt = DateTimeOffset.UtcNow
        };
        await unitOfWork.RefreshTokens.AddAsync(newToken, ct);
        await unitOfWork.SaveChangesAsync(ct);

        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        var accessToken = jwtTokenService.GenerateAccessToken(user, roles);

        return new AuthResult(
            accessToken,
            newRawToken,
            DateTimeOffset.UtcNow.AddMinutes(_jwtOptions.AccessTokenMinutes),
            ToProfile(user, roles));
    }

    public async Task LogoutAsync(string rawRefreshToken, CancellationToken ct = default)
    {
        var tokenHash = jwtTokenService.HashToken(rawRefreshToken);
        var existingToken = await unitOfWork.RefreshTokens.GetByTokenHashAsync(tokenHash, ct);
        if (existingToken is null || existingToken.RevokedAt is not null) return;

        existingToken.RevokedAt = DateTimeOffset.UtcNow;
        unitOfWork.RefreshTokens.Update(existingToken);
        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken ct = default)
    {
        await forgotPasswordValidator.ValidateAndThrowAsync(request, ct);

        var user = await unitOfWork.Users.GetByEmailAsync(request.Email.Trim().ToLowerInvariant(), ct);
        if (user is null || !user.IsActive)
            return;

        var rawToken = jwtTokenService.GenerateRefreshToken();
        var resetToken = new PasswordResetToken
        {
            UserId = user.Id,
            TokenHash = jwtTokenService.HashToken(rawToken),
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(1),
            CreatedAt = DateTimeOffset.UtcNow
        };
        await unitOfWork.PasswordResetTokens.AddAsync(resetToken, ct);
        await unitOfWork.SaveChangesAsync(ct);

        var baseUrl = configuration["Frontend:BaseUrl"]!.TrimEnd('/');
        var resetLink = $"{baseUrl}/reset-password?email={Uri.EscapeDataString(user.Email)}&token={Uri.EscapeDataString(rawToken)}";

        try
        {

            await emailSender.SendAsync(
                user.Email,
                "Reset your Molecule by Makeover password",
                $"<p>We received a request to reset your password.</p><p><a href=\"{resetLink}\">Click here to reset your password</a></p><p>This link will expire in 1 hour. If you did not request this, you can safely ignore this email.</p>",
                ct);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            throw;
        }

    }

    public async Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct = default)
    {
        await resetPasswordValidator.ValidateAndThrowAsync(request, ct);

        var user = await unitOfWork.Users.GetByEmailAsync(request.Email.Trim().ToLowerInvariant(), ct);
        var tokenHash = jwtTokenService.HashToken(request.Token);
        var resetToken = await unitOfWork.PasswordResetTokens.GetByTokenHashAsync(tokenHash, ct);

        if (user is null || resetToken is null || resetToken.UserId != user.Id
            || resetToken.UsedAt is not null || resetToken.ExpiresAt <= DateTimeOffset.UtcNow)
            throw new BadRequestException("Invalid or expired reset token.");

        user.PasswordHash = passwordHasher.Hash(request.NewPassword);
        unitOfWork.Users.Update(user);

        resetToken.UsedAt = DateTimeOffset.UtcNow;
        unitOfWork.PasswordResetTokens.Update(resetToken);

        await RevokeAllActiveTokensForUserAsync(user.Id, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }

    private async Task<AuthResult> IssueTokensAsync(User user, IReadOnlyList<string> roles, string? ipAddress, CancellationToken ct)
    {
        var accessToken = jwtTokenService.GenerateAccessToken(user, roles);
        var rawRefreshToken = jwtTokenService.GenerateRefreshToken();

        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = jwtTokenService.HashToken(rawRefreshToken),
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(_jwtOptions.RefreshTokenDays),
            CreatedByIp = ipAddress,
            CreatedAt = DateTimeOffset.UtcNow
        };
        await unitOfWork.RefreshTokens.AddAsync(refreshToken, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return new AuthResult(
            accessToken,
            rawRefreshToken,
            DateTimeOffset.UtcNow.AddMinutes(_jwtOptions.AccessTokenMinutes),
            ToProfile(user, roles));
    }

    private async Task RevokeAllActiveTokensForUserAsync(Guid userId, CancellationToken ct)
    {
        var activeTokens = unitOfWork.RefreshTokens.Query()
            .Where(t => t.UserId == userId && t.RevokedAt == null)
            .ToList();

        foreach (var token in activeTokens)
        {
            token.RevokedAt = DateTimeOffset.UtcNow;
            unitOfWork.RefreshTokens.Update(token);
        }

        await unitOfWork.SaveChangesAsync(ct);
    }

    private static UserProfileDto ToProfile(User user, IReadOnlyList<string> roles) =>
        new(user.Id, user.Email, user.FirstName, user.LastName, user.PhoneNumber, roles);
}
