using Microsoft.EntityFrameworkCore;
using MoleculeByMakeover.Domain.Entities;
using MoleculeByMakeover.Domain.Interfaces;
using MoleculeByMakeover.Shared.Common;
using MoleculeByMakeover.Shared.Exceptions;

namespace MoleculeByMakeover.Application.Features.Admin;

public class CustomerService(IUnitOfWork unitOfWork) : ICustomerService
{
    public async Task<PaginatedList<CustomerSummaryDto>> GetAllAsync(int page, int pageSize, CancellationToken ct = default)
    {
        var query = unitOfWork.Users.Query().OrderByDescending(u => u.CreatedAt);
        var total = await query.CountAsync(ct);
        var users = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);

        var items = new List<CustomerSummaryDto>();
        foreach (var user in users)
        {
            var orderCount = await unitOfWork.Orders.Query().CountAsync(o => o.UserId == user.Id, ct);
            items.Add(new CustomerSummaryDto(user.Id, user.Email, user.FirstName, user.LastName, user.IsActive, user.CreatedAt, orderCount));
        }

        return PaginatedList<CustomerSummaryDto>.Create(items, total, page, pageSize);
    }

    public async Task<CustomerDetailDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var user = await unitOfWork.Users.GetByIdAsync(id, ct) ?? throw new NotFoundException(nameof(User), id);
        return new CustomerDetailDto(user.Id, user.Email, user.FirstName, user.LastName, user.PhoneNumber, user.IsActive, user.CreatedAt);
    }
}
