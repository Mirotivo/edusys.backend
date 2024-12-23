using System.Diagnostics;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using skillseek.Models;

namespace skillseek.Controllers;

public class BaseController : ControllerBase
{
    protected int GetUserId()
    {
        var userIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
        {
            throw new UnauthorizedAccessException("User ID not found or invalid.");
        }

        return userId;
    }

    // JsonError methods
    protected IActionResult JsonError(string message, IEnumerable<string> errors)
    {
        return new BadRequestObjectResult(new
        {
            Success = false,
            Message = message,
            Errors = errors ?? new List<string>()
        });
    }
    protected IActionResult JsonError(string message, string error)
    {
        return JsonError(message, new List<string> { error });
    }
    protected IActionResult JsonError(string message = "")
    {
        return JsonError(message, new List<string>());
    }

    // JsonOk methods
    protected IActionResult JsonOk(string message, object? data = null)
    {
        return new OkObjectResult(new
        {
            Success = true,
            Message = message,
            Data = data
        });
    }

    protected IActionResult JsonOk(object? data = null)
    {
        return JsonOk("Operation completed successfully.", data);
    }

    protected IActionResult JsonOk()
    {
        return JsonOk("Operation completed successfully.");
    }
}

