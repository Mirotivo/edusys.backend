using System.Threading.Tasks;

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
    Task<PaymentResult> CreatePaymentAsync(decimal amount, string currency, string returnUrl, string cancelUrl);
    Task<string> CreatePayoutAsync(string recipientAccountId, decimal amount, string currency);

    // Update
    Task<PaymentResult> CapturePaymentAsync(string paymentId, string stripeCustomerId, string cardId, decimal amount, string description);

    // Delete / Refund
    Task<string> RefundPaymentAsync(string paymentId, decimal amount);
}

