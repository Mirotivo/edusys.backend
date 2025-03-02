using Backend.Database.Models;
using Backend.Interfaces.Billing;
using Backend.Services.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using PayPal.Api;
using Stripe;

public class PaymentService : IPaymentService
{
    const decimal platformFeeRate = 0.1m;
    private readonly AvanciraDbContext _dbContext;
    private readonly IStripeCardService _cardService;
    private readonly IWalletService _walletService;
    private readonly IPaymentGatewayFactory _paymentGatewayFactory;
    private readonly AppOptions _appOptions;
    private readonly StripeOptions _stripeOptions;
    private readonly ILogger<PaymentService> _logger;
    private readonly INotificationService _notificationService;

    public PaymentService(
        AvanciraDbContext dbContext,
        IStripeCardService cardService,
        IWalletService walletService,
        IPaymentGatewayFactory paymentGatewayFactory,
        IOptions<AppOptions> appOptions,
        IOptions<StripeOptions> stripeOptions,
        ILogger<PaymentService> logger,
        INotificationService notificationService
    )
    {
        _dbContext = dbContext;
        _cardService = cardService;
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
        return await gateway.InitiatePaymentAsync(request.Amount, request.Currency, request.ReturnUrl, request.CancelUrl);
    }

    public async Task<string> CreateAccountLinkAsync(string userId, string gatewayName = "Stripe")
    {
        var user = await _dbContext.Users.FindAsync(userId);
        if (user == null)
            throw new KeyNotFoundException("User not found.");

        var gateway = _paymentGatewayFactory.GetPaymentGateway(gatewayName);
        var accountId = await gateway.CreateStripeAccountAsync(user.Email);

        // Save the Account ID (account.Id) in your database
        user.StripeCustomerId = accountId;
        await _dbContext.SaveChangesAsync();

        return await gateway.CreateAccountLinkAsync(accountId);
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
            var payoutResult = await gateway.InitiatePayoutAsync(user.StripeCustomerId, amount, currency);

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

    public async Task<bool> AddSavedCardAsync(string userId, SaveCardDto request)
    {
        StripeConfiguration.ApiKey = _stripeOptions.ApiKey;

        var user = await _dbContext.Users.FindAsync(userId);
        if (user == null) throw new KeyNotFoundException("User not found.");

        if (string.IsNullOrEmpty(user.StripeCustomerId))
        {
            var customerService = new CustomerService();
            var customer = await customerService.CreateAsync(new CustomerCreateOptions
            {
                Email = user.Email,
                Name = $"{user.FullName}",
            });

            user.StripeCustomerId = customer.Id;
            await _dbContext.SaveChangesAsync();
        }

        var card = await _cardService.CreateAsync(user.StripeCustomerId, new CardCreateOptions
        {
            Source = request.StripeToken,
        });


        var userCard = new UserCard
        {
            UserId = userId,
            CardId = card.Id,
            Last4 = card.Last4,
            ExpMonth = card.ExpMonth,
            ExpYear = card.ExpYear,
            Brand = card.Brand,
            Type = request.Purpose,
        };

        _dbContext.UserCards.Add(userCard);
        await _dbContext.SaveChangesAsync();

        return true;
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

        // Calculate the total amount collected by the user
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

    // TODO : Pagination
    public async Task<IEnumerable<CardDto>> ListSavedCardsAsync(string userId)
    {
        return await _dbContext.UserCards
            .Where(c => c.UserId == userId)
            .Select(c => new CardDto
            {
                Id = c.Id,
                Last4 = c.Last4,
                ExpMonth = c.ExpMonth,
                ExpYear = c.ExpYear,
                Type = c.Brand,
                Purpose = c.Type
            })
            .ToListAsync();
    }


    public async Task<bool> CapturePaymentAsync(CapturePaymentRequestDto request)
    {
        var gateway = _paymentGatewayFactory.GetPaymentGateway(request.Gateway);
        return await gateway.CapturePaymentAsync(request.PaymentId);
    }

    public async Task<Transaction> ExecuteTransactionAsync(string stripeCustomerId, string senderId, string? recipientId, decimal amount, TransactionPaymentType paymentType, string gatewayName = "Stripe")
    {
        using var transactionScope = await _dbContext.Database.BeginTransactionAsync();

        var paymentId = string.Empty;
        try
        {
            var platformFee = amount * platformFeeRate;
            var netAmount = amount - platformFee;
            var paymentDescription = recipientId == null
                ? $"Subscription payment from Sender: {senderId} to Platform"
                : $"Payment from Sender: {senderId} to Recipient: {recipientId}";

            // Step 1: Process the payment with the gateway
            var gateway = _paymentGatewayFactory.GetPaymentGateway(gatewayName);
            var paymentResult = await gateway.ExecutePaymentAsync(stripeCustomerId, amount, paymentDescription);
            if (paymentResult.Status != PaymentResultStatus.Completed)
            {
                throw new Exception("Payment processing failed.");
            }
            paymentId = paymentResult.PaymentId;

            // Step 2: Create the transaction record
            var transaction = new Transaction
            {
                SenderId = senderId,
                RecipientId = recipientId,
                Amount = netAmount,
                PlatformFee = platformFee,
                TransactionDate = DateTime.UtcNow,
                PaymentType = paymentType,
                PaymentMethod = TransactionPaymentMethod.Card,
                Status = TransactionStatus.Completed,
                PaymentId = paymentResult.PaymentId
            };

            await _dbContext.Transactions.AddAsync(transaction);
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
        catch
        {
            // Rollback in case of any failure
            await transactionScope.RollbackAsync();
            if (!string.IsNullOrEmpty(paymentId))
            {
                var gateway = _paymentGatewayFactory.GetPaymentGateway(gatewayName);
                await gateway.RefundPaymentAsync(paymentId, amount);
                _logger.LogInformation($"Payment {paymentId} successfully refunded after failure.");
            }
            throw;
        }
    }

    public async Task ProcessPastBookedLessons()
    {
        var currentDate = DateTime.UtcNow;

        var pastBookedLessons = await _dbContext.Lessons
            .Include(l => l.Listing)
            .Include(l => l.Listing.User)
            .Include(l => l.Student)
            .Where(l => l.Status == LessonStatus.Booked && l.Date < currentDate)
            .ToListAsync();

        foreach (var lesson in pastBookedLessons)
        {
            lesson.Status = LessonStatus.Completed;
            try
            {
                var tutor = lesson.Listing.User;
                if (tutor.PaymentSchedule == UserPaymentSchedule.PerLesson)
                {
                    var platformFee = lesson.Price * platformFeeRate;
                    var netAmount = lesson.Price - platformFee;
                    await CreatePayoutAsync(tutor.Id, netAmount, "AUD");
                    lesson.Status = LessonStatus.Paid;
                }
            }
            catch (Exception ex)
            {
                // Log the failure but continue processing the rest of the lessons
                _logger.LogError(ex, $"Failed to process lesson ID {lesson.Id}. Error: {ex.Message}");
            }

        }

        if (pastBookedLessons.Any())
        {
            await _dbContext.SaveChangesAsync();
        }
    }


    public async Task<bool> RefundPaymentAsync(int transactionId, decimal refundAmount, decimal retainedAmount, string gatewayName = "Stripe")
    {
        using var transactionScope = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            // Retrieve the transaction associated with the payment ID
            var transaction = await _dbContext.Transactions
                .FirstOrDefaultAsync(t => t.Id == transactionId);
            if (transaction == null)
            {
                throw new KeyNotFoundException("Transaction not found for the given Payment ID.");
            }

            // Fetch PaymentId from the Transaction
            var paymentId = transaction?.PaymentId;
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

    public async Task<bool> RemoveCardAsync(string userId, int cardId)
    {
        StripeConfiguration.ApiKey = _stripeOptions.ApiKey;

        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null || string.IsNullOrEmpty(user.StripeCustomerId))
        {
            throw new KeyNotFoundException("User or Stripe customer not found.");
        }

        var card = await _dbContext.UserCards.FirstOrDefaultAsync(c => c.Id == cardId && c.UserId == userId);
        if (card == null)
        {
            throw new KeyNotFoundException("Card not found.");
        }

        var cardService = new CardService();
        await cardService.DeleteAsync(user.StripeCustomerId, card.CardId);

        _dbContext.UserCards.Remove(card);
        await _dbContext.SaveChangesAsync();

        // Check if there are any remaining cards for the user
        var remainingCards = _dbContext.UserCards.Any(c => c.UserId == userId);
        if (!remainingCards)
        {
            // If no cards remain, clear the StripeCustomerId
            user.StripeCustomerId = null;
            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync();
        }

        return true;
    }
}
