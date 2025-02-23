using System.Security.Claims;
using System.Threading.Tasks;
using avancira.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PayPalCheckoutSdk.Orders;
using Stripe;
using Stripe.Checkout;

[ApiController]
[Route("api/payments")]
public class PaymentsAPIController : BaseController
{
    private readonly IPaymentService _paymentService;

    public PaymentsAPIController(
        IPaymentService paymentService
    )
    {
        _paymentService = paymentService;
    }

    //[Authorize]
    //[HttpPost("add-money")]
    //public async Task<IActionResult> AddMoneyToWallet([FromBody] PaymentRequestDto request)
    //{
    //    var userId = GetUserId();

    //    try
    //    {
    //        var result = await _walletService.AddMoneyToWallet(userId, request);
    //        return JsonOk(new { result.PaymentId, result.ApprovalUrl, result.TransactionId });
    //    }
    //    catch (Exception ex)
    //    {
    //        return JsonError("Failed to process payment.", ex.Message);
    //    }
    //}

    [HttpPost("create-payment")]
    public async Task<IActionResult> CreatePayment([FromBody] PaymentRequestDto request)
    {
        try
        {
            var paymentResult = await _paymentService.CreatePaymentAsync(request);

            return JsonOk(new
            {
                success = true,
                paymentId = paymentResult.PaymentId,
                approvalUrl = paymentResult.ApprovalUrl
            });
        }
        catch (Exception ex)
        {
            return JsonError(ex.Message);
        }
    }

    [HttpPost("capture-payment")]
    public async Task<IActionResult> CapturePayment([FromBody] CapturePaymentRequestDto request)
    {
        try
        {
            var success = await _paymentService.CapturePaymentAsync(request);

            if (success)
            {
                // Return success response
                return JsonOk(new
                {
                    success = true,
                    message = "Payment captured and subscription created successfully.",
                });
            }

            return JsonError("Failed to capture payment.");
        }
        catch (Exception ex)
        {
            return JsonError(ex.Message);
        }
    }

    [Authorize]
    [HttpGet("history")]
    public async Task<IActionResult> HistoryAsync()
    {
        var userId = GetUserId();
        var paymentHistory = await _paymentService.GetPaymentHistoryAsync(userId);
        return JsonOk(paymentHistory);
    }

    #region Cards
    [Authorize]
    [HttpPost("save-card")]
    public async Task<IActionResult> SaveCard([FromBody] SaveCardDto request)
    {
        var userId = GetUserId();
        await _paymentService.AddSavedCardAsync(userId, request);
        return JsonOk(new { success = true, message = "Card saved successfully." });
    }

    [Authorize]
    [HttpDelete("remove-card/{Id}")]
    public async Task<IActionResult> RemoveCard(int Id)
    {
        var userId = GetUserId();
        await _paymentService.RemoveCardAsync(userId, Id);
        return JsonOk(new { success = true, message = "Card removed successfully." });
    }

    [Authorize]
    [HttpGet("saved-cards")]
    public async Task<IActionResult> GetSavedCardsAsync()
    {
        var userId = GetUserId();
        var cards = await _paymentService.ListSavedCardsAsync(userId);
        return JsonOk(cards);
    }
    #endregion


    [Authorize]
    [HttpGet("connect-link")]
    public async Task<IActionResult> CreateAccount()
    {
        var userId = GetUserId();

        var url = await _paymentService.CreateAccountLinkAsync(userId);

        return Ok(new { url });
    }

    [Authorize]
    [HttpPost("create-payout")]
    public async Task<IActionResult> CreatePayout([FromBody] CreatePayoutRequest request)
    {
        var userId = GetUserId();
        try
        {
            var payoutId = await _paymentService.CreatePayoutAsync(userId, request.Amount, request.Currency);

            return JsonOk(new
            {
                success = true,
                message = "Payout processed successfully",
                payoutId
            });
        }
        catch (Exception ex)
        {
            return JsonError(ex.Message);
        }
    }
}
