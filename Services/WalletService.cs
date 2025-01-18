using Microsoft.EntityFrameworkCore;

public class WalletService : IWalletService
{
    private readonly AvanciraDbContext _dbContext;
    private readonly IPaymentService _paymentService;
    private readonly ILogger<WalletService> _logger;

    public WalletService(
        AvanciraDbContext dbContext,
        IPaymentService paymentService,
        ILogger<WalletService> logger
    )
    {
        _dbContext = dbContext;
        _paymentService = paymentService;
        _logger = logger;
    }

    public async Task<(string PaymentId, string ApprovalUrl, int TransactionId)> AddMoneyToWallet(string userId, PaymentRequestDto request)
    {
        var user = await _dbContext.Users.FindAsync(userId);
        if (user == null) throw new KeyNotFoundException("User not found.");

        if (request.Amount <= 0)
            throw new ArgumentException("Amount must be greater than zero.");

        if (string.IsNullOrEmpty(user.StripeCustomerId))
            throw new ArgumentException("Stripe Customer ID is missing for the user.");

        // Use PaymentService to process transaction
        var transaction = await _paymentService.ProcessTransactionAsync(
            stripeCustomerId: user.StripeCustomerId,
            senderId: userId,
            recipientId: null, // Platform
            amount: request.Amount,
            paymentType: PaymentType.WalletTopUp,
            gatewayName: request.Gateway
        );

        return (transaction?.PaymentId ?? string.Empty, request.ReturnUrl, transaction?.Id ?? 0);
    }

    public async Task<(decimal Balance, DateTime LastUpdated)> GetWalletBalance(string userId)
    {
        var wallet = await _dbContext.Wallets.FirstOrDefaultAsync(w => w.UserId == userId);
        if (wallet == null)
            throw new InvalidOperationException("Wallet not found.");

        return (wallet.Balance, wallet.UpdatedAt);
    }
}
