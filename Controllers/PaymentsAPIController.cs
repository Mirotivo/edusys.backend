using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PayPalCheckoutSdk.Orders;
using skillseek.Controllers;
using Stripe;
using Stripe.Checkout;
using System.Security.Claims;
using System.Threading.Tasks;

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

    [HttpGet("history")]
    public async Task<IActionResult> HistoryAsync()
    {
        var userId = GetUserId();
        var paymentHistory = await _paymentService.GetPaymentHistoryAsync(userId);
        return JsonOk(paymentHistory);
    }


    [HttpPost("add-paypal-account")]
    public async Task<IActionResult> AddPayPalAccountAsync([FromBody] AddPayPalAccountDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            return Unauthorized();
        }

        await _paymentService.AddPayPalAccountAsync(userId, dto.PayPalEmail);
        return JsonOk("PayPal account added successfully.");
    }


    [HttpPost("save-card")]
    public async Task<IActionResult> SaveCard([FromBody] SaveCardDto request)
    {
        var userId = GetUserId();
        await _paymentService.SaveCardAsync(userId, request);
        return JsonOk(new { success = true, message = "Card saved successfully." });
    }

    [HttpDelete("remove-card/{Id}")]
    public async Task<IActionResult> RemoveCard(int Id)
    {
        var userId = GetUserId();
        await _paymentService.RemoveCardAsync(userId, Id);
        return JsonOk(new { success = true, message = "Card removed successfully." });
    }

    [HttpGet("saved-cards")]
    public async Task<IActionResult> GetSavedCardsAsync()
    {
        var userId = GetUserId();
        var cards = await _paymentService.GetSavedCardsAsync(userId);
        return JsonOk(cards);
    }

}
