using System.Diagnostics;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;

namespace avancira.Controllers;


public class GenericReturnType
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public object? Data { get; set; }
    public IEnumerable<string>? Errors { get; set; }
}

public class BaseController : ControllerBase
{
    protected string GetUserId()
    {
        var userIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        if (userIdClaim == null || string.IsNullOrWhiteSpace(userIdClaim.Value))
        {
            throw new UnauthorizedAccessException("User ID not found or invalid.");
        }

        return userIdClaim.Value;
    }

    // JsonError methods
    protected IActionResult JsonError(string message, IEnumerable<string> errors)
    {
        return new BadRequestObjectResult(new GenericReturnType
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
        return new OkObjectResult(new GenericReturnType
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
