
public interface IPaymentService
{
    Task<PaymentHistoryDto> GetPaymentHistoryAsync(string userId);

    Task<PaymentResult> CreatePaymentAsync(PaymentRequestDto request);
    Task<bool> CapturePaymentAsync(CapturePaymentRequestDto request);
    Task<Transaction> ProcessTransactionAsync(string stripeCustomerId, string senderId, string? recipientId, decimal amount, PaymentType paymentType, string gatewayName = "Stripe");
    Task<bool> RefundPaymentAsync(int transactionId, decimal refundAmount, decimal retainedAmount, string gatewayName = "Stripe");
    Task<string> CreateAccountLinkAsync(string userId, string gatewayName = "Stripe");
    Task<string> CreatePayoutAsync(string sellerId, decimal amount, string currency, string gatewayName = "Stripe");

    Task<bool> SaveCardAsync(string userId, SaveCardDto request);
    Task<bool> RemoveCardAsync(string userId, int cardId);
    Task<IEnumerable<CardDto>> GetSavedCardsAsync(string userId);

    Task ProcessPastBookedLessons();
}
