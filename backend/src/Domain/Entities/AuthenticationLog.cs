using MoleculeByMakeover.Domain.Enums;

namespace MoleculeByMakeover.Domain.Entities;

// Append-only audit trail of login attempts. Deliberately holds no credentials: never the password,
// its hash, or any token - only who tried, from where, and whether it worked.
public class AuthenticationLog
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Null when the attempted email matches no account.</summary>
    public Guid? UserId { get; set; }

    /// <summary>The email as submitted, trimmed and lower-cased.</summary>
    public string Email { get; set; } = default!;

    public bool Succeeded { get; set; }

    /// <summary>Null for successful attempts.</summary>
    public AuthenticationFailureReason? FailureReason { get; set; }

    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }

    /// <summary>UTC.</summary>
    public DateTimeOffset OccurredAt { get; set; }
}
