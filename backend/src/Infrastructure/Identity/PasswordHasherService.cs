using Microsoft.AspNetCore.Identity;
using MoleculeByMakeover.Application.Common.Interfaces;
using MoleculeByMakeover.Domain.Entities;

namespace MoleculeByMakeover.Infrastructure.Identity;

// Uses ASP.NET Core Identity's PBKDF2 hasher directly (Microsoft.Extensions.Identity.Core)
// without pulling in the full Identity/EF Identity-store machinery - Users/Roles stay
// plain Domain entities mapped by our own EF configurations.
public class PasswordHasherService : IPasswordHasherService
{
    private readonly PasswordHasher<User> _hasher = new();

    public string Hash(string password) => _hasher.HashPassword(null!, password);

    public bool Verify(string hash, string providedPassword) =>
        _hasher.VerifyHashedPassword(null!, hash, providedPassword) != PasswordVerificationResult.Failed;
}
