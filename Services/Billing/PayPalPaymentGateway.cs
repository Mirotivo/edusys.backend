using Microsoft.Extensions.Options;
using PayPal;
using PayPalCheckoutSdk.Core;
using PayPalCheckoutSdk.Orders;

public class PayPalPaymentGateway : IPaymentGateway
{
    private readonly PayPalOptions _payPalOptions;
    private readonly PayPalHttpClient _client;
    private readonly ILogger<PayPalPaymentGateway> _logger;

    public PayPalPaymentGateway(
        IOptions<PayPalOptions> options,
        ILogger<PayPalPaymentGateway> logger
    )
    {
        _payPalOptions = options.Value;
        var payPalOptions = options.Value;

        PayPalEnvironment environment = payPalOptions.Environment.ToLower() switch
        {
            "sandbox" => new SandboxEnvironment(payPalOptions.ClientId, payPalOptions.ClientSecret),
            "live" => new LiveEnvironment(payPalOptions.ClientId, payPalOptions.ClientSecret),
            _ => throw new InvalidOperationException("Invalid PayPal environment configuration. Use 'Sandbox' or 'Live'.")
        };

        _client = new PayPalHttpClient(environment);
        _logger = logger;
    }

    public async Task<PaymentResult> CreatePayment(decimal amount, string currency, string returnUrl, string cancelUrl)
    {
        var request = new OrdersCreateRequest();
        request.Prefer("return=representation");
        request.RequestBody(new OrderRequest
        {
            CheckoutPaymentIntent = "CAPTURE",
            ApplicationContext = new ApplicationContext
            {
                ReturnUrl = returnUrl,
                CancelUrl = cancelUrl
            },
            PurchaseUnits = new List<PurchaseUnitRequest>
            {
                new PurchaseUnitRequest
                {
                    AmountWithBreakdown = new AmountWithBreakdown
                    {
                        CurrencyCode = currency,
                        Value = amount.ToString("F2")
                    }
                }
            }
        });

        try
        {
            var response = await _client.Execute(request);

            if (response.StatusCode != System.Net.HttpStatusCode.Created)
            {
                throw new Exception("Failed to create PayPal payment.");
            }

            var order = response.Result<Order>();
            var approvalUrl = order.Links.FirstOrDefault(link => link.Rel == "approve")?.Href;

            if (string.IsNullOrEmpty(approvalUrl))
            {
                throw new Exception("Approval URL not found.");
            }

            return new PaymentResult
            {
                PaymentId = order.Id,
                ApprovalUrl = approvalUrl
            };
        }
        catch (HttpException ex)
        {
            _logger.LogError(ex, "Error creating PayPal payment. Status Code: {StatusCode}", ex.StatusCode);
            throw;
        }
    }

    public async Task<bool> CapturePayment(string paymentId)
    {
        var request = new OrdersCaptureRequest(paymentId);
        request.RequestBody(new OrderActionRequest());

        try
        {
            var response = await _client.Execute(request);

            return response.StatusCode == System.Net.HttpStatusCode.Created;
        }
        catch (HttpException ex)
        {
            _logger.LogError(ex, "Error capturing PayPal payment. Payment ID: {PaymentId}", paymentId);
            throw;
        }
    }

    public async Task<string> RefundPayment(string paymentId, decimal amount)
    {
        await Task.Delay(0);
        throw new NotImplementedException("RefundPayment method is not implemented for PayPal.");
    }

    public async Task<PaymentResult> ProcessPayment(string stripeCustomerId, Transaction transaction)
    {
        await Task.Delay(0);
        throw new NotImplementedException();
        // try
        // {
        //     var request = new OrdersCaptureRequest(transaction.PaymentId);
        //     request.RequestBody(new OrderActionRequest());

        //     var response = await _client.Execute(request);

        //     if (response.StatusCode == System.Net.HttpStatusCode.Created)
        //     {
        //         return true;
        //     }

        //     return false;
        // }
        // catch (HttpException ex)
        // {
        //     return false;
        // }
    }

    public async Task<string> CreatePayoutAsync(string recipientEmail, decimal amount, string currency)
    {
        // Initialize API context using Client ID and Secret
        var config = new Dictionary<string, string>
        {
            { "clientId", _payPalOptions.ClientId },
            { "clientSecret", _payPalOptions.ClientSecret },
            { "mode", _payPalOptions.Environment.ToLower() }
        };
        var accessToken = new PayPal.Api.OAuthTokenCredential(config).GetAccessToken();
        var apiContext = new PayPal.Api.APIContext(accessToken);

        // Create a payout object
        var payout = new PayPal.Api.Payout
        {
            sender_batch_header = new PayPal.Api.PayoutSenderBatchHeader
            {
                sender_batch_id = Guid.NewGuid().ToString(),
                email_subject = "You have received a payment!"
            },
            items = new List<PayPal.Api.PayoutItem>
            {
                new PayPal.Api.PayoutItem
                {
                    recipient_type = PayPal.Api.PayoutRecipientType.EMAIL,
                    receiver = recipientEmail, // Recipient's email address
                    amount = new PayPal.Api.Currency
                    {
                        value = amount.ToString("F2"), // Format amount as a decimal string
                        currency = currency
                    },
                    note = "Thanks for your hard work!",
                    sender_item_id = Guid.NewGuid().ToString()
                }
            }
        };

        try
        {
            // Execute the payout
            var createdPayout = await Task.Run(() => payout.Create(apiContext, false));

            // Return the payout batch ID
            return createdPayout.batch_header.payout_batch_id;
        }
        catch (PayPalException ex)
        {
            // Log the exception and rethrow
            _logger.LogError(ex, "Error creating PayPal payout.");
            throw;
        }
    }

    public Task<string> CreateStripeAccountAsync(string email)
    {
        throw new NotImplementedException();
    }
}
