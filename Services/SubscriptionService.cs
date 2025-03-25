using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public class SubscriptionService : ISubscriptionService
{
    private readonly AvanciraDbContext _dbContext;
    private readonly IPaymentService _paymentService;
    private readonly ILogger<SubscriptionService> _logger;

    public SubscriptionService(
        AvanciraDbContext dbContext,
        IPaymentService paymentService,
        ILogger<SubscriptionService> logger
    )
    {
        _dbContext = dbContext;
        _paymentService = paymentService;
        _logger = logger;
    }


    public async Task<(int SubscriptionId, int TransactionId)> CreateSubscriptionAsync(SubscriptionRequestDto request, string userId)
    {
        // Step 1: Validate User
        var user = await _dbContext.Users.FindAsync(userId);
        if (user == null) throw new KeyNotFoundException("User not found.");

        // Step 2: Determine Billing Frequency
        var billingFrequency = SubscriptionBillingFrequency.Monthly;
        if (!string.IsNullOrEmpty(request.BillingFrequency) &&
            !Enum.TryParse(request.BillingFrequency, true, out billingFrequency))
        {
            billingFrequency = SubscriptionBillingFrequency.Monthly;
        }


        // Step 3: Define Subscription Prices
        const decimal studentMonthlyPrice = 69m;
        const decimal studentAnnualPrice = studentMonthlyPrice * 12; // 12-months cost
        const decimal tutorMonthlyPrice = 99m;
        const decimal tutorAnnualPrice = tutorMonthlyPrice * 12; // 12-months cost
        // Determine base price and duration based on plan type
        decimal basePrice;
        var subscriptionMonths = 0;
        if (billingFrequency == SubscriptionBillingFrequency.Monthly)
        {
            basePrice = request.PaymentType == TransactionPaymentType.StudentMembership ? studentMonthlyPrice : tutorMonthlyPrice;
            subscriptionMonths = 1;
        }
        else // Annual Plan (12 months paid, 3 extra months free)
        {
            basePrice = request.PaymentType == TransactionPaymentType.StudentMembership ? studentAnnualPrice : tutorAnnualPrice;
            subscriptionMonths = 15; // 12 paid + 3 free
        }

        // Step 4: Apply Promo Code (if any)
        var discountApplied = 0.0m;
        var finalAmount = basePrice;
        if (!string.IsNullOrEmpty(request.PromoCode))
        {
            var promo = await _dbContext.PromoCodes
                .Where(p => p.Code == request.PromoCode && p.IsActive && p.ExpiryDate > DateTime.UtcNow)
                .FirstOrDefaultAsync();
            if (promo != null)
            {
                if (promo.DiscountAmount > 0)
                {
                    discountApplied = promo.DiscountAmount;
                }
                else if (promo.DiscountPercentage > 0)
                {
                    discountApplied = (basePrice * (decimal)(promo.DiscountPercentage / 100));
                }
                finalAmount = Math.Max(0, basePrice - discountApplied); // Ensure price doesn't go negative
            }
        }
        // Step 5: Validate Payment Amount
        if (request.Amount != finalAmount)
        {
            throw new InvalidOperationException($"Invalid payment amount. Expected {finalAmount:C}, but received {request.Amount:C}.");
        }

        // Step 6: Create a Transaction BEFORE Capturing Payment
        var transaction = new Transaction
        {
            SenderId = userId,
            RecipientId = null, // Payment to platform
            Amount = finalAmount,
            PlatformFee = 0.0m,
            TransactionDate = DateTime.UtcNow,
            PaymentMethod = request.PaymentMethod,
            PaymentType = request.PaymentType,
            Status = TransactionStatus.Created, // Initial status
            Description = "Subscription Payment",
            PayPalPaymentId = request.PayPalPaymentId, // If using PayPal
            StripeCustomerId = user.StripeCustomerId // If using Stripe
        };

        await _dbContext.Transactions.AddAsync(transaction);
        await _dbContext.SaveChangesAsync();


        // Step 7: Capture Payment
        transaction = await _paymentService.CapturePaymentAsync(transaction.Id, request.PaymentMethod.ToString());

        // Step 8: Create or Update Subscription
        var subscriptionId = 0;
        if (transaction.Status == TransactionStatus.Completed)
        {
            // Check if there's an existing active subscription
            var existingSubscription = await _dbContext.Subscriptions
                .AsTracking()
                .FirstOrDefaultAsync(s => s.UserId == userId);
            if (existingSubscription != null)
            {
                // Update existing subscription
                var previousBillingFrequency = existingSubscription.BillingFrequency;
                existingSubscription.BillingFrequency = billingFrequency;
                existingSubscription.NextBillingDate = DateTime.UtcNow.AddMonths(subscriptionMonths);
                existingSubscription.Amount = finalAmount;
                existingSubscription.CancellationDate = null; // Reactivating

                _dbContext.Subscriptions.Update(existingSubscription);
                await _dbContext.SaveChangesAsync();

                // Log change in history
                var history = new SubscriptionHistory
                {
                    SubscriptionId = existingSubscription.Id,
                    BillingFrequency = billingFrequency,
                    StartDate = existingSubscription.StartDate,
                    NextBillingDate = existingSubscription.NextBillingDate,
                    Amount = existingSubscription.Amount,
                    ChangeDate = DateTime.UtcNow
                };

                _dbContext.SubscriptionHistories.Add(history);
                await _dbContext.SaveChangesAsync();

                subscriptionId = existingSubscription.Id;
            }
            else
            {
                // Create a new subscription
                var subscription = new Subscription
                {
                    UserId = userId,
                    BillingFrequency = billingFrequency,
                    StartDate = DateTime.UtcNow,
                    NextBillingDate = DateTime.UtcNow.AddMonths(subscriptionMonths),
                    Amount = finalAmount
                };

                _dbContext.Subscriptions.Add(subscription);
                await _dbContext.SaveChangesAsync();

                // Log new subscription history
                var history = new SubscriptionHistory
                {
                    SubscriptionId = subscription.Id,
                    BillingFrequency = billingFrequency,
                    StartDate = subscription.StartDate,
                    NextBillingDate = subscription.NextBillingDate,
                    Amount = subscription.Amount,
                    ChangeDate = DateTime.UtcNow
                };

                _dbContext.SubscriptionHistories.Add(history);
                await _dbContext.SaveChangesAsync();

                subscriptionId = subscription.Id;
            }
        }

        return (subscriptionId, transaction.Id);
    }


    public async Task<bool> HasActiveSubscriptionAsync(string userId)
    {
        return await _dbContext.Subscriptions
            .AnyAsync(s => s.UserId == userId && s.CancellationDate == null);
    }

    public async Task<PagedResult<Subscription>> ListUserSubscriptionsAsync(string userId, int page, int pageSize)
    {
        var queryable = _dbContext.Subscriptions
            .Where(s => s.UserId == userId);

        // Get total count before pagination
        var totalResults = await queryable.CountAsync();

        // Apply pagination
        var lessons = await queryable
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var results = lessons.ToList();

        return new PagedResult<Subscription>
        {
            TotalResults = totalResults,
            Page = page,
            PageSize = pageSize,
            Results = results
        };
    }

    public async Task<PromoCode> ValidatePromoCode(string promoCode)
    {
        var promo = await _dbContext.PromoCodes
            .Where(p => p.Code == promoCode && p.IsActive && p.ExpiryDate > DateTime.UtcNow)
            .FirstOrDefaultAsync();
        return promo;
    }

    // TODO: Pagination.
    public async Task<SubscriptionDetailsDto?> FetchSubscriptionDetailsAsync(string userId)
    {
        // Get the latest active subscription (or most recent cancelled subscription)
        var subscription = await _dbContext.Subscriptions
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.NextBillingDate)
            .FirstOrDefaultAsync();

        if (subscription == null)
            return null;

        // Get all historical records for all user subscriptions
        var history = await _dbContext.SubscriptionHistories
            .Where(h => _dbContext.Subscriptions
                .Where(s => s.UserId == userId)
                .Select(s => s.Id)
                .Contains(h.SubscriptionId))
            .OrderByDescending(h => h.ChangeDate)
            .ToListAsync();

        return new SubscriptionDetailsDto
        {
            BillingFrequency = subscription.BillingFrequency.ToString(),
            StartDate = subscription.StartDate,
            NextBillingDate = subscription.NextBillingDate,
            NextBillingAmount = subscription.Amount,
            Status = subscription.Status.ToString(),
            SubscriptionHistory = history.Select(h => new SubscriptionHistoryDto
            {
                Action = GetActionDescription(h),
                ChangeDate = h.ChangeDate,
                BillingFrequency = h.BillingFrequency.ToString(),
                Amount = h.Amount,
                Status = h.Status.ToString()
            }).ToList()
        };
    }


    /// <summary>
    /// Changes the billing frequency of an active subscription.
    /// </summary>
    public async Task<bool> ChangeBillingFrequencyAsync(string userId, SubscriptionBillingFrequency newFrequency)
    {
        var subscription = await _dbContext.Subscriptions
            .Where(s => s.UserId == userId && s.CancellationDate == null)
            .AsTracking()
            .FirstOrDefaultAsync();

        if (subscription == null)
            return false;

        // Log the previous state in history
        var history = new SubscriptionHistory
        {
            SubscriptionId = subscription.Id,
            BillingFrequency = subscription.BillingFrequency,
            StartDate = subscription.StartDate,
            CancellationDate = DateTime.UtcNow, // This marks the change moment
            NextBillingDate = subscription.NextBillingDate,
            Amount = subscription.Amount,
            //PaymentMethod = subscription.PaymentMethod,
            //PaymentMethodId = subscription.PaymentMethodId,
            ChangeDate = DateTime.UtcNow
        };
        _dbContext.SubscriptionHistories.Add(history);

        // Update the subscription with new frequency
        subscription.BillingFrequency = newFrequency;
        subscription.NextBillingDate = DateTime.UtcNow.AddMonths(newFrequency == SubscriptionBillingFrequency.Monthly ? 1 : 12);

        _dbContext.Subscriptions.Update(subscription);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Cancels an active subscription and logs it in history.
    /// </summary>
    public async Task<bool> CancelSubscriptionAsync(string userId)
    {
        var subscription = await _dbContext.Subscriptions
            .Where(s => s.UserId == userId && s.CancellationDate == null)
            .AsTracking()
            .FirstOrDefaultAsync();

        if (subscription == null)
            return false;

        // Log the cancellation in history
        var history = new SubscriptionHistory
        {
            SubscriptionId = subscription.Id,
            BillingFrequency = subscription.BillingFrequency,
            StartDate = subscription.StartDate,
            CancellationDate = DateTime.UtcNow,
            NextBillingDate = subscription.NextBillingDate,
            Amount = subscription.Amount,
            //PaymentMethod = subscription.PaymentMethod,
            //PaymentMethodId = subscription.PaymentMethodId,
            ChangeDate = DateTime.UtcNow
        };
        _dbContext.SubscriptionHistories.Add(history);

        // Mark subscription as canceled
        subscription.CancellationDate = DateTime.UtcNow;
        //subscription.Status = SubscriptionStatus.Canceled;

        _dbContext.Subscriptions.Update(subscription);
        await _dbContext.SaveChangesAsync();
        return true;
    }



    private string GetActionDescription(SubscriptionHistory history)
    {
        if (history.CancellationDate.HasValue)
            return "Cancelled Subscription";

        if (history.BillingFrequency == SubscriptionBillingFrequency.Monthly)
            return "Switched to Monthly Plan";

        if (history.BillingFrequency == SubscriptionBillingFrequency.Yearly)
            return "Switched to Yearly Plan";

        return "Subscription Updated";
    }
}

