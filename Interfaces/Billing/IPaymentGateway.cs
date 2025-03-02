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
    // Create
    Task<PaymentResult> InitiatePaymentAsync(decimal amount, string currency, string returnUrl, string cancelUrl);
    Task<string> CreateStripeAccountAsync(string email);
    Task<string> CreateAccountLinkAsync(string userId);
    Task<string> InitiatePayoutAsync(string stripeCustomerId, decimal amount, string currency);

    // Update
    Task<bool> CapturePaymentAsync(string paymentId);
    Task<PaymentResult> ExecutePaymentAsync(string stripeCustomerId, decimal amount, string description);

    // Delete / Refund
    Task<string> RefundPaymentAsync(string paymentId, decimal amount);
}
