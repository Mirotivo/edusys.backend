
public interface IPaymentService
{
    // Create
    Task<PaymentResult> CreatePaymentAsync(PaymentRequestDto request);
    Task<string> CreateAccountLinkAsync(string userId, string gatewayName = "Stripe");
    Task<string> CreatePayoutAsync(string sellerId, decimal amount, string currency, string gatewayName = "Stripe");
    Task<bool> AddSavedCardAsync(string userId, SaveCardDto request);


    // Read
    Task<PaymentHistoryDto> GetPaymentHistoryAsync(string userId);
    Task<IEnumerable<CardDto>> ListSavedCardsAsync(string userId);

    // Update
    Task<bool> CapturePaymentAsync(CapturePaymentRequestDto request);
    Task<Transaction> ExecuteTransactionAsync(string stripeCustomerId, string senderId, string? recipientId, decimal amount, PaymentType paymentType, string gatewayName = "Stripe");
    Task ProcessPastBookedLessons();

    // Refunds & Deletions
    Task<bool> RefundPaymentAsync(int transactionId, decimal refundAmount, decimal retainedAmount, string gatewayName = "Stripe");
    Task<bool> RemoveCardAsync(string userId, int cardId);
}
