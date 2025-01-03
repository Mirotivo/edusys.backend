using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;

public class StripePaymentGateway : IPaymentGateway
{
    private readonly StripeOptions _stripeOptions;
    private readonly ILogger<StripePaymentGateway> _logger;

    public StripePaymentGateway(
        IOptions<StripeOptions> stripeOptions,
        ILogger<StripePaymentGateway> logger
    )
    {
        _stripeOptions = stripeOptions.Value;
        _logger = logger;
    }

    public async Task<PaymentResult> CreatePayment(decimal amount, string currency, string returnUrl, string cancelUrl)
    {
        StripeConfiguration.ApiKey = _stripeOptions.ApiKey;

        var options = new SessionCreateOptions
        {
            PaymentMethodTypes = new List<string> { "card" },
            LineItems = new List<SessionLineItemOptions>
            {
                new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = currency,
                        ProductData = new SessionLineItemPriceDataProductDataOptions { Name = "Order Payment" },
                        UnitAmount = (long)(amount * 100),
                    },
                    Quantity = 1,
                },
            },
            Mode = "payment",
            SuccessUrl = returnUrl,
            CancelUrl = cancelUrl,
        };

        var service = new SessionService();
        var session = await service.CreateAsync(options);
        _logger.LogInformation("Stripe payment session created. Payment ID: {PaymentId}, URL: {ApprovalUrl}", session.Id, session.Url);

        return new PaymentResult
        {
            PaymentId = session.Id,
            ApprovalUrl = session.Url
        };
    }

    public async Task<bool> CapturePayment(string paymentId)
    {
        // Stripe automatically captures payments when using Sessions
        return await Task.FromResult(true);
    }


    public async Task<string> RefundPayment(string paymentId, decimal amount)
    {
        try
        {
            StripeConfiguration.ApiKey = _stripeOptions.ApiKey;

            var refundService = new RefundService();
            var refundOptions = new RefundCreateOptions
            {
                Charge = paymentId,  // Use Charge ID
                Amount = (long)(amount * 100), // Convert amount to cents
            };

            var refund = await refundService.CreateAsync(refundOptions);

            _logger.LogInformation("Refund successful. Refund ID: {RefundId} for Payment ID: {PaymentId}", refund.Id, paymentId);
            return refund.Id; // Return the Refund ID
        }
        catch (StripeException ex)
        {
            // Handle Stripe-specific exceptions
            _logger.LogError(ex, "Stripe refund failed for Payment ID: {PaymentId}", paymentId);
            throw;
        }
        catch (Exception ex)
        {
            // Handle other exceptions
            _logger.LogError(ex, "Refund failed for Payment ID: {PaymentId}", paymentId);
            throw;
        }
    }

    public async Task<PaymentResult> ProcessPayment(string stripeCustomerId, Transaction transaction)
    {
        try
        {
            StripeConfiguration.ApiKey = _stripeOptions.ApiKey;

            var options = new ChargeCreateOptions
            {
                Amount = (long)((transaction.Amount + transaction.PlatformFee) * 100),
                Currency = "AUD",
                Customer = stripeCustomerId,
                Description = $"Payment for Lesson ID {transaction.Id}",
            };

            var service = new ChargeService();
            var charge = await service.CreateAsync(options);

            if (charge.Status == "succeeded")
            {
                _logger.LogInformation("Stripe payment succeeded. Charge ID: {ChargeId} for Transaction ID: {TransactionId}", charge.Id, transaction.Id);
                return new PaymentResult
                {
                    PaymentId = charge.Id, // Payment ID from Stripe
                    ApprovalUrl = null,    // Not applicable for direct charge payments
                    Status = PaymentResultStatus.Completed
                };
            }
            else
            {
                _logger.LogWarning("Stripe payment failed. Charge ID: {ChargeId} for Transaction ID: {TransactionId}", charge.Id, transaction.Id);
                // Handle other statuses gracefully
                return new PaymentResult
                {
                    PaymentId = charge.Id,
                    ApprovalUrl = null,
                    Status = PaymentResultStatus.Failed
                };
            }
        }
        catch (Exception ex)
        {
            // Log error
            _logger.LogError(ex, "Stripe payment processing failed for Transaction ID: {TransactionId}", transaction.Id);

            // Return a failed result for consistency
            return new PaymentResult
            {
                PaymentId = null,
                ApprovalUrl = null,
                Status = PaymentResultStatus.Failed
            };
        }
    }

    public async Task<string> CreatePayoutAsync(string recipientEmail, decimal amount, string currency)
    {
        StripeConfiguration.ApiKey = _stripeOptions.SecretKey;

        // Hardcoded Stripe account creation for simplicity
        var accountService = new AccountService();
        var account = await accountService.GetAsync("acct_1QTYlcCEKBsiEvKg"); // Replace with the connected account ID

        if (account.Requirements.CurrentlyDue.Count > 0)
        {
            Console.WriteLine("Missing Fields:");
            foreach (var field in account.Requirements.CurrentlyDue)
            {
                Console.WriteLine(field);
            }
        }

        // Step 1: Create a Stripe Connected Account
        var accountOptions = new AccountCreateOptions
        {
            Type = "custom",
            Country = "AU", // Australia (ISO 3166-1 alpha-2 country code)
            Capabilities = new AccountCapabilitiesOptions
            {
                Transfers = new AccountCapabilitiesTransfersOptions { Requested = true },
            },
            BusinessType = "individual",
            Individual = new AccountIndividualOptions
            {
                FirstName = "Amr",
                LastName = "Badr",
                Email = "amr.mostafa@live.com",
            },
        };

        var connectedAccount = await accountService.CreateAsync(accountOptions);

        // For simplicity, use the newly created account ID
        var stripeAccountId = connectedAccount.Id;

        // Step 2: Attach a Bank Account to the Connected Account
        var accountUpdateOptions = new AccountUpdateOptions
        {
            ExternalAccount = new AccountBankAccountOptions
            {
                Country = "AU",
                Currency = "AUD",
                AccountHolderName = "Amr Badr",
                AccountHolderType = "individual",
                RoutingNumber = "110000",      // Replace with test routing number
                AccountNumber = "000123456",  // Replace with test account number
            },
        };

        await accountService.UpdateAsync(stripeAccountId, accountUpdateOptions);

        // Step 3: Create a Payout
        var payoutOptions = new PayoutCreateOptions
        {
            Amount = (long?)amount, // Amount in cents
            Currency = currency,
        };

        var payoutService = new PayoutService();
        var payout = await payoutService.CreateAsync(payoutOptions, new RequestOptions
        {
            StripeAccount = stripeAccountId, // Connected account for the payout
        });

        return payout.Id; // Return the payout ID for confirmation

        // // Retrieve seller's Stripe account ID
        // var stripeAccountId = await _dbContext.Users
        //     .Where(u => u.Id == sellerId)
        //     .Select(u => u.StripeCustomerId)
        //     .FirstOrDefaultAsync();
        // if (string.IsNullOrEmpty(stripeAccountId))
        // {
        //     throw new Exception("Seller's Stripe account not found.");
        // }

        // try
        // {
        //     // Configure Stripe API
        //     StripeConfiguration.ApiKey = _stripeOptions.ApiKey;

        //     // Create the payout
        //     var payoutOptions = new PayoutCreateOptions
        //     {
        //         Amount = amount, // Amount in cents
        //         Currency = currency,
        //     };

        //     var payoutService = new PayoutService();
        //     var payout = await payoutService.CreateAsync(payoutOptions, new RequestOptions
        //     {
        //         StripeAccount = stripeAccountId // Connect payout to the seller's account
        //     });

        //     return payout.Id; // Return the payout ID for reference
        // }
        // catch (StripeException ex)
        // {
        //     throw new Exception($"Stripe error: {ex.Message}");
        // }
    }
}
