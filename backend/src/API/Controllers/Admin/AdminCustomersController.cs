using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoleculeByMakeover.Application.Features.Admin;
using MoleculeByMakeover.Shared.Constants;

namespace MoleculeByMakeover.API.Controllers.Admin;

[Route("api/v1/admin/customers")]
[Authorize(Roles = RoleNames.Admin)]
public class AdminCustomersController(ICustomerService customerService) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await customerService.GetAllAsync(page, pageSize, ct);
        return Ok(new { result.Items, result.Page, result.PageSize, result.TotalCount, result.TotalPages });
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CustomerDetailDto>> GetById(Guid id, CancellationToken ct) =>
        Ok(await customerService.GetByIdAsync(id, ct));
}
