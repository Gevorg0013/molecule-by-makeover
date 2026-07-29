namespace MoleculeByMakeover.Application.Features.Admin;

public record CustomerSummaryDto(Guid Id, string Email, string FirstName, string LastName, bool IsActive, DateTimeOffset CreatedAt, int OrderCount);

public record CustomerDetailDto(Guid Id, string Email, string FirstName, string LastName, string? PhoneNumber, bool IsActive, DateTimeOffset CreatedAt);
