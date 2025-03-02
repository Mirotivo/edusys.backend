using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace avancira.Controllers;

[Route("api/wallets")]
[ApiController]
public class WalletsAPIController : BaseController
{
    private readonly IWalletService _walletService;

    public WalletsAPIController(
        IWalletService walletService
    )
    {
        _walletService = walletService;
    }

    // Read
    [Authorize]
    [HttpGet("balance")]
    public async Task<IActionResult> GetWalletBalance()
    {
        var userId = GetUserId();

        try
        {
            var result = await _walletService.GetWalletBalanceAsync(userId);
            return JsonOk(new { Balance = result.Balance, LastUpdated = result.LastUpdated });
        }
        catch (Exception ex)
        {
            return JsonError("Failed to fetch wallet balance.", ex.Message);
        }
    }
}

