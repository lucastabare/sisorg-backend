using Microsoft.AspNetCore.Mvc;
using Sisorg.Api.ViewModels;

namespace Sisorg.Api.Controllers;

/// <summary>
/// Base controller with helper methods for consistent responses.
/// </summary>
[ApiController]
public abstract class BaseController : ControllerBase
{
    protected ActionResult ErrorBadRequest(string message, string code = "VALIDATION_ERROR")
    {
        return BadRequest(new ErrorViewModel { Code = code, Message = message, CorrelationId = Guid.NewGuid().ToString() });
    }

    protected ActionResult ErrorNotFound(string message)
    {
        return NotFound(new ErrorViewModel { Code = "NOT_FOUND", Message = message, CorrelationId = Guid.NewGuid().ToString() });
    }
}
