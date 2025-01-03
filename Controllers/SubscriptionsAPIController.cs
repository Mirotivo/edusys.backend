using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace skillseek.Controllers;

[Route("api/subscriptions")]
[ApiController]
public class SubscriptionsAPIController : BaseController
{

    private readonly ISubscriptionService _subscriptionService;

    public SubscriptionsAPIController(
        ISubscriptionService subscriptionService
    )
    {
        _subscriptionService = subscriptionService;
    }

    [Authorize]
    [HttpGet("check-active")]
    public async Task<IActionResult> CheckActiveSubscription()
    {
        var userId = GetUserId();
        var hasActiveSubscription = await _subscriptionService.CheckActiveSubscription(userId);
        return JsonOk(new { IsActive = hasActiveSubscription });
    }

    [Authorize]
    [HttpPost("create")]
    public async Task<IActionResult> CreateSubscription([FromBody] SubscriptionRequestDto request)
    {
        try
        {
            var userId = GetUserId();
            var (subscriptionId, transactionId) = await _subscriptionService.CreateSubscription(request, userId);
            return JsonOk(new { SubscriptionId = subscriptionId, TransactionId = transactionId });
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                Message = "An error occurred while creating the listing.",
                Error = ex.Message
            });
        }
    }

    [Authorize]
    [HttpGet("")]
    public async Task<IActionResult> GetUserSubscriptions()
    {
        var userId = GetUserId();
        var subscriptions = await _subscriptionService.GetUserSubscriptions(userId);

        if (subscriptions == null || subscriptions.Count == 0)
            return NotFound("No subscriptions found.");

        return JsonOk(subscriptions);
    }

}

