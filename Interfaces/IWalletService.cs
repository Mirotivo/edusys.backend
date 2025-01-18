public interface IWalletService
{
    Task<(string PaymentId, string ApprovalUrl, int TransactionId)> AddMoneyToWallet(string userId, PaymentRequestDto request);
    Task<(decimal Balance, DateTime LastUpdated)> GetWalletBalance(string userId);
}
