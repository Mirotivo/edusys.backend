using Microsoft.EntityFrameworkCore;
using Stripe;

public enum PaymentResultStatus
{
    Completed,
    Failed
}
public class PaymentResult
{
    public string? PaymentId { get; set; }
    public string? ApprovalUrl { get; set; } // For gateways like Stripe or PayPal
    public PaymentResultStatus Status { get; set; } // Optional: Add status information if needed
}

public interface IPaymentGateway
{
    Task<PaymentResult> CreatePayment(decimal amount, string currency, string returnUrl, string cancelUrl);
    Task<bool> CapturePayment(string paymentId);
    Task<string> RefundPayment(string paymentId, decimal amount);
    Task<PaymentResult> ProcessPayment(string stripeCustomerId, decimal amount, string description);
    Task<string> CreatePayoutAsync(string stripeCustomerId, decimal amount, string currency);
    Task<string> CreateAccountLinkAsync(string userId);
    Task<string> CreateStripeAccountAsync(string email);
}
