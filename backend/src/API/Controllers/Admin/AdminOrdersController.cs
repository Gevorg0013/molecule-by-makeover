using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoleculeByMakeover.Application.Features.Ordering;
using MoleculeByMakeover.Shared.Constants;

namespace MoleculeByMakeover.API.Controllers.Admin;

[Route("api/v1/admin/orders")]
[Authorize(Roles = RoleNames.Admin)]
public class AdminOrdersController(IOrderService orderService) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult> GetAll([FromQuery] OrderQueryParameters query, CancellationToken ct)
    {
        var result = await orderService.GetAllAdminAsync(query, ct);
        return Ok(new { result.Items, result.Page, result.PageSize, result.TotalCount, result.TotalPages });
    }

    [HttpGet("{orderNumber}")]
    public async Task<ActionResult<OrderDetailDto>> GetByOrderNumber(string orderNumber, CancellationToken ct) =>
        Ok(await orderService.GetByOrderNumberAsync(orderNumber, CurrentUserId, true, ct));

    [HttpPut("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, UpdateOrderStatusRequest request, CancellationToken ct)
    {
        await orderService.UpdateStatusAsync(id, request, ct);
        return NoContent();
    }
}

[Route("api/v1/admin/coupons")]
[Authorize(Roles = RoleNames.Admin)]
public class AdminCouponsController(ICouponService couponService) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<CouponDto>>> GetAll(CancellationToken ct) => Ok(await couponService.GetAllAsync(ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CouponDto>> GetById(Guid id, CancellationToken ct) => Ok(await couponService.GetByIdAsync(id, ct));

    [HttpPost]
    public async Task<ActionResult> Create(CouponUpsertRequest request, CancellationToken ct)
    {
        var id = await couponService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, CouponUpsertRequest request, CancellationToken ct)
    {
        await couponService.UpdateAsync(id, request, ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await couponService.DeleteAsync(id, ct);
        return NoContent();
    }
}
