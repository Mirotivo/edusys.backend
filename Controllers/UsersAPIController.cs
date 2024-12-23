using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System;
using skillseek.Controllers;

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
        if (!await _userService.RegisterUser(model))
        {
            return JsonError("Registration failed", "A user with this email address already exists.");
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
        var success = await _userService.ResetPassword(model.Token, model.NewPassword);
        if (!success)
        {
            return JsonError("Invalid or expired token.");
        }

        return JsonOk(new { success = true, message = "Password reset successfully." });
    }

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

    [HttpGet("diploma-status")]
    public async Task<IActionResult> GetDiplomaStatus()
    {
        var userId = GetUserId();
        var status = await _userService.GetDiplomaStatusAsync(userId);
        return JsonOk(new { status });
    }

    [HttpGet("compensation-percentage")]
    public async Task<IActionResult> GetCompensationPercentage()
    {
        var userId = GetUserId();
        var percentage = await _userService.GetCompensationPercentageAsync(userId);
        return JsonOk(percentage);
    }




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

    [HttpPut("compensation-percentage")]
    public async Task<IActionResult> UpdateCompensationPercentage([FromBody] CompensationUpdateDto dto)
    {
        var userId = GetUserId();
        await _userService.UpdateCompensationPercentageAsync(userId, dto.Percentage);
        return JsonOk(new { message = "Compensation percentage updated successfully." });
    }




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
