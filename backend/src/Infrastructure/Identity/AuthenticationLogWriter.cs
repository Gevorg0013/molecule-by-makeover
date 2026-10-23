using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MoleculeByMakeover.Application.Common.Interfaces;
using MoleculeByMakeover.Domain.Entities;
using MoleculeByMakeover.Infrastructure.Persistence;

namespace MoleculeByMakeover.Infrastructure.Identity;

// Writes through its own DbContext scope rather than the request's IUnitOfWork, so a failed audit
// insert can't leave a broken entity tracked in the login's unit of work (or roll back its tokens).
public class AuthenticationLogWriter(
    IServiceScopeFactory scopeFactory,
    IHttpContextAccessor httpContextAccessor,
    ILogger<AuthenticationLogWriter> logger) : IAuthenticationLogWriter
{
    public const int EmailMaxLength = 256;
    public const int IpAddressMaxLength = 45; // longest textual IPv6 form
    public const int UserAgentMaxLength = 512;

    public async Task WriteAsync(AuthenticationLog entry, CancellationToken ct = default)
    {
        try
        {
            entry.Email = Truncate(entry.Email.Trim().ToLowerInvariant(), EmailMaxLength)!;
            entry.IpAddress = Truncate(entry.IpAddress, IpAddressMaxLength);
            entry.UserAgent = Truncate(entry.UserAgent ?? httpContextAccessor.HttpContext?.Request.Headers.UserAgent.ToString(), UserAgentMaxLength);
            if (entry.OccurredAt == default) entry.OccurredAt = DateTimeOffset.UtcNow;

            using var scope = scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            dbContext.AuthenticationLogs.Add(entry);
            // Not tied to the request's token: a client hanging up mid-login shouldn't drop the record.
            await dbContext.SaveChangesAsync(CancellationToken.None);
        }
        catch (Exception ex)
        {
            // Only non-identifying fields: the email stays out of the application log.
            logger.LogError(ex, "Failed to write authentication log (succeeded {Succeeded}, reason {FailureReason}, user {UserId})",
                entry.Succeeded, entry.FailureReason, entry.UserId);
        }
    }

    private static string? Truncate(string? value, int maxLength) =>
        string.IsNullOrEmpty(value) ? null : value.Length <= maxLength ? value : value[..maxLength];
}
