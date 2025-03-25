using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Interfaces.Billing;
using Backend.Services.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public class PaymentService : IPaymentService
{
    const decimal platformFeeRate = 0.1m;
    private readonly AvanciraDbContext _dbContext;
    private readonly IStripeCardService _stripeCardService;
    private readonly IWalletService _walletService;
    private readonly IPaymentGatewayFactory _paymentGatewayFactory;
    private readonly AppOptions _appOptions;
    private readonly StripeOptions _stripeOptions;
    private readonly ILogger<PaymentService> _logger;
    private readonly INotificationService _notificationService;

    public PaymentService(
        AvanciraDbContext dbContext,
        IStripeCardService stripeCardService,
        IWalletService walletService,
        IPaymentGatewayFactory paymentGatewayFactory,
        IOptions<AppOptions> appOptions,
        IOptions<StripeOptions> stripeOptions,
        ILogger<PaymentService> logger,
        INotificationService notificationService
    )
    {
        _dbContext = dbContext;
        _stripeCardService = stripeCardService;
        _walletService = walletService;
        _paymentGatewayFactory = paymentGatewayFactory;
        _appOptions = appOptions.Value;
        _stripeOptions = stripeOptions.Value;
        _logger = logger;
        _notificationService = notificationService;
    }

    public async Task<PaymentResult> CreatePaymentAsync(PaymentRequestDto request)
    {
        var gateway = _paymentGatewayFactory.GetPaymentGateway(request.Gateway);
        return await gateway.CreatePaymentAsync(request.Amount, request.Currency, request.ReturnUrl, request.CancelUrl);
    }

    public async Task<string> CreatePayoutAsync(string userId, decimal amount, string currency, string gatewayName = "Stripe")
    {
        var user = await _dbContext.Users.FindAsync(userId);
        if (user == null)
            throw new KeyNotFoundException("User not found.");

        using var transactionScope = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            // Step 1: Fetch the user's wallet to validate balance
            var wallet = await _dbContext.Wallets.FirstOrDefaultAsync(w => w.UserId == userId);

            if (wallet == null)
            {
                _logger.LogWarning($"Wallet for User with ID {userId} not found.");
                throw new InvalidOperationException("Wallet not found.");
            }

            // Step 2: Check if the wallet has sufficient funds
            if (wallet.Balance < amount)
            {
                _logger.LogWarning($"Insufficient balance for User ID {userId}. Requested: {amount}, Available: {wallet.Balance}");
                throw new InvalidOperationException("Insufficient wallet balance for payout.");
            }

            // Step 3: Proceed with payout using the payment gateway
            var gateway = _paymentGatewayFactory.GetPaymentGateway(gatewayName);
            var recipientAccountId = (user.PaymentGateway == "PayPal") ? user.PayPalAccountId
                : user.StripeConnectedAccountId;
            var payoutResult = await gateway.CreatePayoutAsync(recipientAccountId ?? string.Empty, amount, currency);

            // Step 4: Use _walletService to update the wallet balance
            await _walletService.ModifyWalletBalanceAsync(
                userId,
                -amount,
                $"Payout of {amount:C} processed successfully."
            );

            // Commit the transaction
            await transactionScope.CommitAsync();

            _logger.LogInformation($"Payout of {amount:C} successfully processed for User ID {userId}.");

            var eventData = new PayoutProcessedEvent
            {
                UserId = userId,
                Amount = amount,
                Currency = currency,
                WalletBalance = wallet.Balance - amount,
                ProcessedAt = DateTime.UtcNow
            };

            await _notificationService.NotifyAsync(NotificationEvent.PayoutProcessed, eventData);

            return payoutResult;
        }
        catch (Exception ex)
        {
            // Rollback in case of any failure
            await transactionScope.RollbackAsync();

            _logger.LogError(ex, $"Failed to process payout for User ID {userId}.");

            var eventData = new PayoutFailedEvent
            {
                UserId = userId,
                Amount = amount,
                Currency = currency,
                ErrorMessage = ex.Message,
                AttemptedAt = DateTime.UtcNow
            };

            await _notificationService.NotifyAsync(NotificationEvent.PaymentFailed, eventData);
            throw new Exception("Payout processing failed. Please try again.");
        }
    }


    // TODO: Pagination.
    public async Task<PaymentHistoryDto> GetPaymentHistoryAsync(string userId)
    {
        // Fetch all relevant transactions involving the user
        var transactions = await _dbContext.Transactions
            .Where(t => t.SenderId == userId || t.RecipientId == userId)
            .Where(t => t.Status == TransactionStatus.Completed)
            .OrderByDescending(t => t.TransactionDate) // Order by date
            .Select(t => new TransactionDto
            {
                Id = t.Id,
                SenderId = t.SenderId,
                SenderName = t.Sender.FullName,
                RecipientId = t.RecipientId,
                RecipientName = t.Recipient != null ? t.Recipient.FullName : "Platform",
                Amount = t.SenderId == userId ? -(t.Amount + t.PlatformFee) : (t.Amount + t.PlatformFee),
                PlatformFee = t.PlatformFee,
                Net = t.SenderId == userId
                    ? -t.Amount
                    : t.Amount,
                Status = t.Status.ToString(),
                TransactionDate = t.TransactionDate,
                Date = t.TransactionDate.ToString("yyyy-MM-dd HH:mm:ss"),
                Description = t.RecipientId == null
                    ? $"Payment to Platform"
                    : t.SenderId == userId
                        ? $"Transferred to {t.Recipient.FullName}"
                        : $"Received from {t.Sender.FullName}",
                TransactionType = t.RecipientId == null
                    ? "To the Platform"
                    : t.SenderId == userId
                        ? "Sent to User"
                        : "Received from User",
                Type = t.RecipientId == null
                    ? "transfer" // Transactions involving the platform are transfers
                    : "payment" // Transactions between users are payments
            })
            .ToListAsync();

        // Filter transactions with RecipientId == null as invoices
        var invoices = transactions.Where(t => t.RecipientId == null).ToList();
        // Exclude invoices from the transactions list
        transactions = transactions.Where(t => t.RecipientId != null).ToList();

        // Fetch wallet balance
        var walletBalance = _dbContext.Wallets
            .Where(w => w.UserId == userId)
            .Select(w => w.Balance)
            .FirstOrDefault();

        // Calculate the total netAmount collected by the user
        var totalAmountCollected = _dbContext.Transactions
            .Where(t => t.RecipientId == userId && t.Status == TransactionStatus.Completed)
            .AsEnumerable()
            .Sum(t => t.Amount);

        // Response with chronological transaction history and balance
        return new PaymentHistoryDto
        {
            WalletBalance = walletBalance,
            TotalAmountCollected = totalAmountCollected,
            Invoices = invoices,
            Transactions = transactions
        };
    }


    public async Task<Transaction> CapturePaymentAsync(int transactionId, string gatewayName = "Stripe", string recipientId = "")
    {
        using var transactionScope = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            // Step 1: Fetch the existing transaction
            var transaction = await _dbContext.Transactions
                .AsTracking()
                .FirstOrDefaultAsync(t => t.Id == transactionId && t.Status == TransactionStatus.Created);

            if (transaction == null)
            {
                throw new Exception($"Transaction {transactionId} not found or not in 'Created' status.");
            }

            // Extract details from the transaction
            var paymentId = transaction.PayPalPaymentId;
            var netAmount = transaction.Amount + transaction.PlatformFee;
            var customerId = transaction.StripeCustomerId;
            var cardId = transaction.StripeCardId;
            var paymentDescription = transaction.Description;

            if (string.IsNullOrEmpty(paymentId) && string.IsNullOrEmpty(customerId))
            {
                throw new Exception($"Transaction {transactionId} is missing required payment details.");
            }

            // Step 2: Process the payment with the payment gateway
            var gateway = _paymentGatewayFactory.GetPaymentGateway(gatewayName);
            var paymentResult = await gateway.CapturePaymentAsync(paymentId, customerId, cardId, netAmount, paymentDescription);

            // Step 3: Update transaction status
            transaction.Status = paymentResult.Status == PaymentResultStatus.Completed
                ? TransactionStatus.Completed
                : TransactionStatus.Failed;

            _dbContext.Transactions.Update(transaction);
            await _dbContext.SaveChangesAsync();

            // Step 3: Update recipient's wallet balance
            if (!string.IsNullOrEmpty(recipientId))
            {
                await _walletService.ModifyWalletBalanceAsync(recipientId, netAmount, $"Payment for transaction {transaction.Id}");
            }

            // Commit the transaction
            await transactionScope.CommitAsync();

            return transaction;
        }
        catch (Exception ex)
        {
            // Rollback in case of any failure
            await transactionScope.RollbackAsync();

            // Refund the payment if a failure occurred
            var transaction = await _dbContext.Transactions.FirstOrDefaultAsync(t => t.Id == transactionId);
            if (transaction != null && !string.IsNullOrEmpty(transaction.PayPalPaymentId))
            {
                var gateway = _paymentGatewayFactory.GetPaymentGateway(gatewayName);
                await gateway.RefundPaymentAsync(transaction.PayPalPaymentId, transaction.Amount + transaction.PlatformFee);
                _logger.LogInformation($"Payment {transaction.PayPalPaymentId} successfully refunded after failure.");
            }

            _logger.LogError(ex, "Error capturing payment for transaction {TransactionId}", transactionId);
            throw new Exception($"Payment capture failed for transaction {transactionId}.", ex);
        }
    }

    public async Task<bool> RefundPaymentAsync(int transactionId, decimal refundAmount, decimal retainedAmount, string gatewayName = "Stripe")
    {
        using var transactionScope = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            // Retrieve the transaction associated with the payment ID
            var transaction = await _dbContext.Transactions
                .AsTracking()
                .FirstOrDefaultAsync(t => t.Id == transactionId);
            if (transaction == null)
            {
                throw new KeyNotFoundException("Transaction not found for the given Payment ID.");
            }

            // Fetch PayPalPaymentId from the Transaction
            var paymentId = transaction?.PayPalPaymentId;
            if (string.IsNullOrEmpty(paymentId))
            {
                throw new InvalidOperationException("Payment ID not found for this transaction.");
            }

            var gateway = _paymentGatewayFactory.GetPaymentGateway(gatewayName);

            // Perform the refund through the payment gateway
            await gateway.RefundPaymentAsync(paymentId, refundAmount);

            // Update the transaction record in the database
            transaction.IsRefunded = true;
            transaction.RefundedAt = DateTime.UtcNow;
            transaction.RefundAmount = refundAmount;
            transaction.Status = TransactionStatus.Refunded;

            _dbContext.Transactions.Update(transaction);
            if (!string.IsNullOrEmpty(transaction.RecipientId))
            {
                var recipientId = transaction.RecipientId;
                var refundNetAmount = refundAmount * (transaction.Amount / (transaction.Amount + transaction.PlatformFee));
                await _walletService.ModifyWalletBalanceAsync(recipientId, -refundNetAmount, $"Refund for transaction {transactionId}");
            }

            _dbContext.Transactions.Update(transaction);
            await _dbContext.SaveChangesAsync();

            // Commit the transaction
            await transactionScope.CommitAsync();

            _logger.LogInformation($"Refund of {refundAmount:C} processed for transaction {transaction.Id}.");
            return true;
        }
        catch (Exception ex)
        {
            // Rollback in case of any failure
            await transactionScope.RollbackAsync();

            _logger.LogError($"Refund failed for transaction {transactionId}: {ex.Message}");
            throw new Exception($"Refund failed: {ex.Message}");
        }
    }
}

