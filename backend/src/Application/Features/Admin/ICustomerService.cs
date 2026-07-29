using MoleculeByMakeover.Shared.Common;

namespace MoleculeByMakeover.Application.Features.Admin;

public interface ICustomerService
{
    Task<PaginatedList<CustomerSummaryDto>> GetAllAsync(int page, int pageSize, CancellationToken ct = default);
    Task<CustomerDetailDto> GetByIdAsync(Guid id, CancellationToken ct = default);
}
