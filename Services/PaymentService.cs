using Backend.Interfaces.Billing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Stripe;

public class PaymentService : IPaymentService
{
    const decimal platformFeeRate = 0.1m;
    private readonly AvanciraDbContext _dbContext;
    private readonly IStripeCardService _cardService;
    private readonly IPaymentGatewayFactory _paymentGatewayFactory;
    private readonly StripeOptions _stripeOptions;
    private readonly ILogger<PaymentService> _logger;
    private readonly INotificationService _notificationService;

    public PaymentService(
        AvanciraDbContext dbContext,
        IStripeCardService cardService,
        IPaymentGatewayFactory paymentGatewayFactory,
        IOptions<StripeOptions> stripeOptions,
        ILogger<PaymentService> logger,
        INotificationService notificationService
    )
    {
        _dbContext = dbContext;
        _cardService = cardService;
        _paymentGatewayFactory = paymentGatewayFactory;
        _stripeOptions = stripeOptions.Value;
        _logger = logger;
        _notificationService = notificationService;
    }

    public async Task<PaymentResult> CreatePaymentAsync(PaymentRequestDto request)
    {
        var gateway = _paymentGatewayFactory.GetPaymentGateway(request.Gateway);
        return await gateway.CreatePayment(request.Amount, request.Currency, request.ReturnUrl, request.CancelUrl);
    }

    public async Task<bool> CapturePaymentAsync(CapturePaymentRequestDto request)
    {
        var gateway = _paymentGatewayFactory.GetPaymentGateway(request.Gateway);
        return await gateway.CapturePayment(request.PaymentId);
    }

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
                SenderName = t.Sender.FirstName,
                RecipientId = t.RecipientId,
                RecipientName = t.Recipient != null ? t.Recipient.FirstName : "Platform",
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
                        ? $"Transferred to {t.Recipient.FirstName}"
                        : $"Received from {t.Sender.FirstName}",
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


    public async Task<Transaction> ProcessTransactionAsync(string stripeCustomerId, string senderId, string? recipientId, decimal amount, PaymentType paymentType, string gatewayName = "Stripe")
    {
        var platformFee = amount * platformFeeRate;
        var netAmount = amount - platformFee;

        // Create a transaction
        var transaction = new Transaction
        {
            SenderId = senderId,
            RecipientId = recipientId,
            Amount = netAmount,
            PlatformFee = platformFee,
            TransactionDate = DateTime.UtcNow,
            PaymentType = paymentType,
            PaymentMethod = PaymentMethod.Card,
            Status = TransactionStatus.Pending
        };

        await _dbContext.Transactions.AddAsync(transaction);
        await _dbContext.SaveChangesAsync();

        // Process the payment
        var gateway = _paymentGatewayFactory.GetPaymentGateway(gatewayName);
        var paymentResult = await gateway.ProcessPayment(stripeCustomerId, transaction);

        if (paymentResult.Status == PaymentResultStatus.Completed)
        {
            transaction.Status = TransactionStatus.Completed;
            transaction.PaymentId = paymentResult.PaymentId;

            // Update recipient's wallet
            if (!string.IsNullOrEmpty(recipientId))
            {
                await UpdateWalletBalance(recipientId, netAmount, $"Payment for transaction {transaction.Id}");
            }
        }
        else
        {
            transaction.Status = TransactionStatus.Failed;
            throw new Exception("Payment processing failed.");
        }

        await _dbContext.SaveChangesAsync();
        return transaction;
    }

    public async Task<bool> RefundPaymentAsync(int transactionId, decimal refundAmount, decimal retainedAmount, string gatewayName = "Stripe")
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
            throw new InvalidOperationException("Payment ID not found for this lesson.");
        }

        var gateway = _paymentGatewayFactory.GetPaymentGateway(gatewayName);


        try
        {
            // Perform the refund through the payment gateway
            await gateway.RefundPayment(paymentId, refundAmount);

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
                await UpdateWalletBalance(recipientId, -refundNetAmount, $"Refund for transaction {transactionId}");
            }

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation($"Refund of {refundAmount:C} processed for transaction {transaction.Id}.");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Refund failed for transaction {transaction.Id}: {ex.Message}");
            throw new Exception($"Refund failed: {ex.Message}");
        }
    }

    public async Task<string> CreatePayoutAsync(string userId, decimal amount, string currency)
    {
        // Step 1: Fetch the user's wallet
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
        var gateway = _paymentGatewayFactory.GetPaymentGateway("PayPal");
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            _logger.LogWarning($"User with ID {userId} not found.");
            throw new KeyNotFoundException("User not found.");
        }

        string payoutResult;
        try
        {
            payoutResult = await gateway.CreatePayoutAsync(user.Email, amount, currency);

            // Step 4: Deduct the payout amount from the wallet
            wallet.Balance -= amount;
            wallet.UpdatedAt = DateTime.UtcNow;

            // Log the wallet update
            _dbContext.Add(new WalletLog
            {
                WalletId = wallet.Id,
                AmountChanged = -amount,
                NewBalance = wallet.Balance,
                Reason = $"Payout of {amount:C} processed successfully.",
                CreatedAt = DateTime.UtcNow
            });

            _dbContext.Wallets.Update(wallet);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation($"Payout of {amount:C} successfully processed for User ID {userId}.");

            // Step 5: Notify the user about the payout
            await _notificationService.NotifyAsync(
                userId.ToString(),
                NotificationEvent.PayoutProcessed,
                $"Your payout of {amount:C} has been successfully processed.",
                new
                {
                    Amount = amount,
                    Currency = currency,
                    WalletBalance = wallet.Balance,
                    ProcessedAt = DateTime.UtcNow
                }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to process payout for User ID {userId}.");
            throw new Exception("Payout processing failed. Please try again.");
        }

        return payoutResult;
    }


    public async Task<bool> SaveCardAsync(string userId, SaveCardDto request)
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
                Name = $"{user.FirstName} {user.LastName}",
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

    public async Task<IEnumerable<CardDto>> GetSavedCardsAsync(string userId)
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
            var tutor = lesson.Listing.User;
            if (tutor.PaymentSchedule == PaymentSchedule.PerLesson)
            {
                var platformFee = lesson.Price * platformFeeRate;
                var netAmount = lesson.Price - platformFee;
                await CreatePayoutAsync(tutor.Id, netAmount, "AUD");
            }
        }

        if (pastBookedLessons.Any())
        {
            await _dbContext.SaveChangesAsync();
        }
    }


    private async Task UpdateWalletBalance(string userId, decimal amount, string reason)
    {
        var wallet = await _dbContext.Wallets.FirstOrDefaultAsync(w => w.UserId == userId);

        if (wallet != null)
        {
            wallet.Balance += amount;
            wallet.UpdatedAt = DateTime.UtcNow;

            // Log the wallet update
            _dbContext.Add(new WalletLog
            {
                WalletId = wallet.Id,
                AmountChanged = amount,
                NewBalance = wallet.Balance,
                Reason = reason,
                CreatedAt = DateTime.UtcNow
            });

            _dbContext.Wallets.Update(wallet);
        }
        else
        {
            wallet = new Wallet
            {
                UserId = userId,
                Balance = amount,
                UpdatedAt = DateTime.UtcNow
            };

            await _dbContext.Wallets.AddAsync(wallet);
            await _dbContext.SaveChangesAsync();

            // Log the wallet creation
            _dbContext.Add(new WalletLog
            {
                WalletId = wallet.Id,
                AmountChanged = amount,
                NewBalance = wallet.Balance,
                Reason = $"Wallet created with initial balance: {amount:C}"
            });
        }

        await _dbContext.SaveChangesAsync();
    }
}
