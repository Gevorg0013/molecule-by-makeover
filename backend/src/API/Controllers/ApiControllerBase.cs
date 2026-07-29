using Microsoft.AspNetCore.Mvc;
using MoleculeByMakeover.Application.Common.Interfaces;

namespace MoleculeByMakeover.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
    private ICurrentUserService? _currentUser;
    protected ICurrentUserService CurrentUser => _currentUser ??= HttpContext.RequestServices.GetRequiredService<ICurrentUserService>();

    protected Guid CurrentUserId => CurrentUser.UserId ?? throw new InvalidOperationException("No authenticated user.");
}
