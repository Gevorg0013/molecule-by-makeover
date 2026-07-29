using MoleculeByMakeover.Domain.Entities;

namespace MoleculeByMakeover.Application.Common.Interfaces;

public interface IJwtTokenService
{
    string GenerateAccessToken(User user, IEnumerable<string> roles);
    string GenerateRefreshToken();
    string HashToken(string rawToken);
}

public interface IPasswordHasherService
{
    string Hash(string password);
    bool Verify(string hash, string providedPassword);
}
