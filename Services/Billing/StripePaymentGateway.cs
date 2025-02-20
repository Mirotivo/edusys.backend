using Microsoft.AspNetCore.Builder.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;

public class StripePaymentGateway : IPaymentGateway
{
    private readonly AppOptions _appOptions;
    private readonly StripeOptions _stripeOptions;
    private readonly ILogger<StripePaymentGateway> _logger;

    public StripePaymentGateway(
        IOptions<AppOptions> appOptions,
        IOptions<StripeOptions> stripeOptions,
        ILogger<StripePaymentGateway> logger
    )
    {
        _appOptions = appOptions.Value;
        _stripeOptions = stripeOptions.Value;
        _logger = logger;
    }

    public async Task<PaymentResult> InitiatePaymentAsync(decimal amount, string currency, string returnUrl, string cancelUrl)
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

    public async Task<bool> CapturePaymentAsync(string paymentId)
    {
        // Stripe automatically captures payments when using Sessions
        return await Task.FromResult(true);
    }


    public async Task<string> RefundPaymentAsync(string paymentId, decimal amount)
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

    public async Task<PaymentResult> ExecutePaymentAsync(string stripeCustomerId, decimal amount, string description)
    {
        try
        {
            StripeConfiguration.ApiKey = _stripeOptions.ApiKey;

            var options = new ChargeCreateOptions
            {
                Amount = (long)(amount * 100),
                Currency = "AUD",
                Customer = stripeCustomerId,
                Description = description,
            };

            var service = new ChargeService();
            var charge = await service.CreateAsync(options);

            if (charge.Status == "succeeded")
            {
                _logger.LogInformation("Stripe payment succeeded. Charge ID: {ChargeId}", charge.Id);
                return new PaymentResult
                {
                    PaymentId = charge.Id, // Payment ID from Stripe
                    ApprovalUrl = null,    // Not applicable for direct charge payments
                    Status = PaymentResultStatus.Completed
                };
            }
            else
            {
                _logger.LogWarning("Stripe payment failed. Charge ID: {ChargeId}", charge.Id);
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
            _logger.LogError(ex, "Stripe payment processing failed");

            // Return a failed result for consistency
            return new PaymentResult
            {
                PaymentId = null,
                ApprovalUrl = null,
                Status = PaymentResultStatus.Failed
            };
        }
    }

    private async Task<Transfer> TransferFundsToUserAsync(string stripeCustomerId, decimal amount, string currency)
    {
        StripeConfiguration.ApiKey = _stripeOptions.ApiKey;

        var transferService = new TransferService();
        var transferOptions = new TransferCreateOptions
        {
            Amount = (long)(amount * 100), // Amount in cents
            Currency = currency, // e.g., "aud"
            Destination = stripeCustomerId, // The connected Stripe account ID
            Description = "Transfer to connected account",
        };

        return await transferService.CreateAsync(transferOptions);
    }


    public async Task<string> InitiatePayoutAsync(string stripeCustomerId, decimal amount, string currency)
    {
        StripeConfiguration.ApiKey = _stripeOptions.ApiKey;

        var accountService = new AccountService();
        var account = await accountService.GetAsync(stripeCustomerId);
        if (!account.PayoutsEnabled)
            throw new InvalidOperationException("Payouts are not enabled for this account.");

        var bankAccounts = account.ExternalAccounts.Data
            .OfType<BankAccount>()
            .Where(ba => ba.Currency == currency);
        if (!bankAccounts.Any())
            throw new InvalidOperationException($"No external bank account found for currency '{currency}'.");


        var transfer = await TransferFundsToUserAsync(
            stripeCustomerId,
            amount,
            currency
        );


        var balanceService = new BalanceService();
        var balance = await balanceService.GetAsync(new RequestOptions { StripeAccount = stripeCustomerId });

        var availableBalance = balance.Available.FirstOrDefault(b => b.Currency == currency);
        if (availableBalance == null || availableBalance.Amount < amount)
            throw new InvalidOperationException($"Insufficient funds in the Stripe account for currency '{currency}'. Current balance: {availableBalance?.Amount ?? 0}.");

        var options = new PayoutCreateOptions
        {
            Amount = (long)(amount * 100),
            Currency = currency,
        };
        var payoutService = new PayoutService();
        var payout = await payoutService.CreateAsync(options, new RequestOptions { StripeAccount = stripeCustomerId });
        _logger.LogInformation("Payout of {Amount} succeeded. Payout ID: {PayoutId}", amount, payout.Id);
        return payout.Id;
    }

    public async Task<string> CreateStripeAccountAsync(string email)
    {
        StripeConfiguration.ApiKey = _stripeOptions.ApiKey;

        var options = new AccountCreateOptions
        {
            Type = "custom", // Use "custom" for full control over payouts
            Email = email,
            Capabilities = new AccountCapabilitiesOptions
            {
                Transfers = new AccountCapabilitiesTransfersOptions { Requested = true },
            },
        };

        var service = new AccountService();
        var account = await service.CreateAsync(options);

        return account.Id;
    }

    public async Task<string> CreateAccountLinkAsync(string accountId)
    {
        StripeConfiguration.ApiKey = _stripeOptions.ApiKey;

        var options = new AccountLinkCreateOptions
        {
            Account = accountId,
            RefreshUrl = $"{_appOptions.FrontEndUrl}/profile?section=payments&detail=receiving",
            ReturnUrl = $"{_appOptions.FrontEndUrl}/profile?section=payments&detail=receiving",
            Type = "account_onboarding",
        };
        var service = new AccountLinkService();
        var link = await service.CreateAsync(options);
        return link.Url;

    }
}
