public interface IWalletService
{
    Task<(string PaymentId, string ApprovalUrl, int TransactionId)> AddMoneyToWallet(int userId, PaymentRequestDto request);
    Task<(decimal Balance, DateTime LastUpdated)> GetWalletBalance(int userId);
}
