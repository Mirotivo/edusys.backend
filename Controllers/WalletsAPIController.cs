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

    [Authorize]
    [HttpPost("add-money")]
    public async Task<IActionResult> AddMoneyToWallet([FromBody] PaymentRequestDto request)
    {
        var userId = GetUserId();

        try
        {
            var result = await _walletService.AddMoneyToWallet(userId, request);
            return JsonOk(new { result.PaymentId, result.ApprovalUrl, result.TransactionId });
        }
        catch (Exception ex)
        {
            return JsonError("Failed to process payment.", ex.Message);
        }
    }

    [Authorize]
    [HttpGet("balance")]
    public async Task<IActionResult> GetWalletBalance()
    {
        var userId = GetUserId();

        try
        {
            var result = await _walletService.GetWalletBalance(userId);
            return JsonOk(new { Balance = result.Balance, LastUpdated = result.LastUpdated });
        }
        catch (Exception ex)
        {
            return JsonError("Failed to fetch wallet balance.", ex.Message);
        }
    }
}

