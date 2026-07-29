using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoleculeByMakeover.Application.Features.Ordering;
using MoleculeByMakeover.Shared.Constants;

namespace MoleculeByMakeover.API.Controllers;

[Authorize]
public class OrdersController(IOrderService orderService) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult> GetMyOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await orderService.GetMyOrdersAsync(CurrentUserId, page, pageSize, ct);
        return Ok(new { result.Items, result.Page, result.PageSize, result.TotalCount, result.TotalPages });
    }

    [HttpGet("{orderNumber}")]
    public async Task<ActionResult<OrderDetailDto>> GetByOrderNumber(string orderNumber, CancellationToken ct) =>
        Ok(await orderService.GetByOrderNumberAsync(orderNumber, CurrentUserId, CurrentUser.IsInRole(RoleNames.Admin), ct));
}
