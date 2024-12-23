using Microsoft.EntityFrameworkCore;

public class WalletService : IWalletService
{
    private readonly skillseekDbContext _dbContext;
    private readonly IPaymentService _paymentService;

    public WalletService(
        skillseekDbContext dbContext,
        IPaymentService paymentService
    )
    {
        _dbContext = dbContext;
        _paymentService = paymentService;
    }

    public async Task<(string PaymentId, string ApprovalUrl, int TransactionId)> AddMoneyToWallet(int userId, PaymentRequestDto request)
    {
        var user = await _dbContext.Users.FindAsync(userId);
        if (user == null) throw new KeyNotFoundException("User not found.");

        if (request.Amount <= 0)
            throw new ArgumentException("Amount must be greater than zero.");

        // Use PaymentService to process transaction
        var transaction = await _paymentService.ProcessTransactionAsync(
            stripeCustomerId: user.StripeCustomerId,
            senderId: userId,
            recipientId: null, // Platform
            amount: request.Amount,
            paymentType: PaymentType.WalletTopUp,
            gatewayName: request.Gateway
        );

        return (transaction.PaymentId, request.ReturnUrl, transaction.Id);
    }

    public async Task<(decimal Balance, DateTime LastUpdated)> GetWalletBalance(int userId)
    {
        var wallet = await _dbContext.Wallets.FirstOrDefaultAsync(w => w.UserId == userId);
        if (wallet == null)
            throw new InvalidOperationException("Wallet not found.");

        return (wallet.Balance, wallet.UpdatedAt);
    }
}
