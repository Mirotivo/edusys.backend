
public interface IPaymentService
{
    Task<PaymentResult> CreatePaymentAsync(PaymentRequestDto request);
    Task<bool> CapturePaymentAsync(CapturePaymentRequestDto request);
    Task<PaymentHistoryDto> GetPaymentHistoryAsync(int userId);

    Task<bool> SaveCardAsync(int userId, SaveCardDto request);
    Task<bool> RemoveCardAsync(int userId, int cardId);
    Task<IEnumerable<CardDto>> GetSavedCardsAsync(int userId);

    Task AddPayPalAccountAsync(string userId, string payPalEmail);

    Task ProcessPastBookedLessons();
    Task<Transaction> ProcessTransactionAsync(
        string stripeCustomerId,
        int senderId, 
        int? recipientId, 
        decimal amount, 
        PaymentType paymentType, 
        string gatewayName = "Stripe");

    Task<bool> RefundPaymentAsync(int transactionId, decimal refundAmount, decimal retainedAmount, string gatewayName = "Stripe");
}
