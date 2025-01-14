
public interface IPaymentService
{
    Task<PaymentResult> CreatePaymentAsync(PaymentRequestDto request);
    Task<bool> CapturePaymentAsync(CapturePaymentRequestDto request);
    Task<PaymentHistoryDto> GetPaymentHistoryAsync(string userId);

    Task<Transaction> ProcessTransactionAsync(string stripeCustomerId, string senderId, string? recipientId, decimal amount, PaymentType paymentType, string gatewayName = "Stripe");
    Task<bool> RefundPaymentAsync(int transactionId, decimal refundAmount, decimal retainedAmount, string gatewayName = "Stripe");
    Task<string> CreatePayoutAsync(string sellerId, decimal amount, string currency);

    Task<bool> SaveCardAsync(string userId, SaveCardDto request);
    Task<bool> RemoveCardAsync(string userId, int cardId);
    Task<IEnumerable<CardDto>> GetSavedCardsAsync(string userId);

    Task ProcessPastBookedLessons();
}
