using MoleculeByMakeover.Domain.Entities;

namespace MoleculeByMakeover.Application.Common.Interfaces;

public interface IAuthenticationLogWriter
{
    /// <summary>
    /// Persists a login attempt. Never throws: an audit-log failure must not change the outcome of
    /// the login itself. Fills in UserAgent from the current request when the caller leaves it null.
    /// </summary>
    Task WriteAsync(AuthenticationLog entry, CancellationToken ct = default);
}
