using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using avancira.Controllers;

[Route("api/users")]
[ApiController]
public class UsersAPIController : BaseController
{
    private readonly IUserService _userService;

    public UsersAPIController(
        IUserService userService
    )
    {
        _userService = userService;
    }


    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        var country = HttpContext.Items["Country"]?.ToString() ?? "AU";
        var (isSuccess, error) = await _userService.RegisterUser(model, country);
        if (!isSuccess)
        {
            return JsonError("Registration failed", error);
        }
        return JsonOk();
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        var result = await _userService.LoginUser(model);
        if (result == null)
        {
            return Unauthorized();
        }

        return JsonOk(new { token = result.Value.Token, roles = result.Value.Roles });
    }

    [HttpPost("request-reset-password")]
    public async Task<IActionResult> RequestPasswordReset([FromBody] ResetPasswordRequestDto model)
    {
        var success = await _userService.SendPasswordResetEmail(model.Email);
        if (!success)
        {
            return NotFound("User with this email address not found.");
        }

        return JsonOk(new { success = true, message = "Password reset link sent to your email." });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto model)
    {
        var success = await _userService.ResetPassword(model.Email, model.Token, model.NewPassword);
        if (!success)
        {
            return JsonError("Invalid or expired token.");
        }

        return JsonOk(new { success = true, message = "Password reset successfully." });
    }

    [Authorize]
    [HttpPost("submit-diploma")]
    public async Task<IActionResult> SubmitDiploma([FromForm] IFormFile diplomaFile)
    {
        var userId = GetUserId();
        if (diplomaFile == null)
        {
            return JsonError("No diploma file provided.");
        }

        var success = await _userService.SubmitDiplomaAsync(userId, diplomaFile);
        return success ? JsonOk(new { message = "Diploma submitted successfully." }) : JsonError("Failed to submit diploma.");
    }



    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetAsync()
    {
        var userId = GetUserId();
        var user = await _userService.GetUser(userId);
        if (user == null)
        {
            throw new UnauthorizedAccessException("User not found.");
        }
        return JsonOk(user);
    }

    [HttpGet("by-token/{recommendationToken}")]
    public async Task<IActionResult> GetUserByToken(string recommendationToken)
    {
        var user = await _userService.GetUserByToken(recommendationToken);
        if (user == null)
        {
            return NotFound("User not found.");
        }

        return JsonOk(user);
    }

    [Authorize]
    [HttpGet("diploma-status")]
    public async Task<IActionResult> GetDiplomaStatus()
    {
        var userId = GetUserId();
        var status = await _userService.GetDiplomaStatusAsync(userId);
        return JsonOk(new { status });
    }

    [Authorize]
    [HttpGet("compensation-percentage")]
    public async Task<IActionResult> GetCompensationPercentage()
    {
        var userId = GetUserId();
        var percentage = await _userService.GetCompensationPercentageAsync(userId);
        return JsonOk(percentage);
    }




    [Authorize]
    [HttpPut("me")]
    public async Task<IActionResult> UpdateAsync([FromForm] UserDto updatedUser)
    {
        var userId = GetUserId();
        if (!await _userService.UpdateUser(userId, updatedUser))
        {
            return NotFound("User not found.");
        }

        return JsonOk(new { success = true, message = "Profile updated successfully." });
    }

    [Authorize]
    [HttpPut("compensation-percentage")]
    public async Task<IActionResult> UpdateCompensationPercentage([FromBody] CompensationUpdateDto dto)
    {
        var userId = GetUserId();
        await _userService.UpdateCompensationPercentageAsync(userId, dto.Percentage);
        return JsonOk(new { message = "Compensation percentage updated successfully." });
    }

    [Authorize]
    [HttpGet("payment-schedule")]
    public async Task<IActionResult> GetPaymentSchedule()
    {
        var userId = GetUserId();
        var paymentSchedule = await _userService.GetPaymentScheduleAsync(userId);

        if (paymentSchedule == null)
        {
            return NotFound("User not found.");
        }

        return JsonOk(paymentSchedule.Value);
    }

    [Authorize]
    [HttpPut("payment-schedule")]
    public async Task<IActionResult> UpdatePaymentSchedule([FromBody] PaymentScheduleDto dto)
    {
        var userId = GetUserId();
        var success = await _userService.UpdatePaymentScheduleAsync(userId, dto.PaymentSchedule);

        if (!success)
        {
            return NotFound("User not found or update failed.");
        }

        return JsonOk(new { message = "Payment schedule updated successfully." });
    }



    [Authorize]
    [HttpDelete("me")]
    public async Task<IActionResult> DeleteAccount()
    {
        var userId = GetUserId();
        var success = await _userService.DeleteAccountAsync(userId);
        if (!success)
        {
            return NotFound(new { success = false, message = "User not found." });
        }

        return JsonOk(new { success = true, message = "Account deleted successfully." });
    }
}
