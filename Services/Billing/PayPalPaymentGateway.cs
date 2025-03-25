using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
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
        IOptions<PayPalOptions> payPalOptions,
        ILogger<PayPalPaymentGateway> logger
    )
    {
        _payPalOptions = payPalOptions.Value;
        var payPalOptionsValue = payPalOptions.Value;

        PayPalEnvironment environment = payPalOptionsValue.Environment.ToLower() switch
        {
            "sandbox" => new SandboxEnvironment(payPalOptionsValue.ClientId, payPalOptionsValue.ClientSecret),
            "live" => new LiveEnvironment(payPalOptionsValue.ClientId, payPalOptionsValue.ClientSecret),
            _ => throw new InvalidOperationException("Invalid PayPal environment configuration. Use 'Sandbox' or 'Live'.")
        };

        _client = new PayPalHttpClient(environment);
        _logger = logger;
    }

    public async Task<PaymentResult> CreatePaymentAsync(decimal amount, string currency, string returnUrl, string cancelUrl)
    {
        var request = new OrdersCreateRequest();
        request.Prefer("return=representation");
        request.RequestBody(new OrderRequest
        {
            CheckoutPaymentIntent = "CAPTURE",
            ApplicationContext = new ApplicationContext
            {
                BrandName = "Avancira",
                ShippingPreference = "NO_SHIPPING",
                UserAction = "PAY_NOW",
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

    public async Task<PaymentResult> CapturePaymentAsync(string paymentId, string stripeCustomerId, string cardId, decimal amount, string description)
    {
        var request = new OrdersCaptureRequest(paymentId);
        request.RequestBody(new OrderActionRequest());

        try
        {
            var response = await _client.Execute(request);

            return new PaymentResult
            {
                PaymentId = paymentId,
                ApprovalUrl = null,
                Status = response.StatusCode == System.Net.HttpStatusCode.Created ? PaymentResultStatus.Completed : PaymentResultStatus.Failed
            };

        }
        catch (HttpException ex)
        {
            _logger.LogError(ex, "Error capturing PayPal payment. Payment ID: {PaymentId}", paymentId);
            throw;
        }
    }

    public async Task<string> RefundPaymentAsync(string paymentId, decimal amount)
    {
        await Task.Delay(0);
        throw new NotImplementedException("RefundPayment method is not implemented for PayPal.");
    }


    public async Task<string> CreatePayoutAsync(string recipientAccountId, decimal amount, string currency)
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
                    recipient_type = PayPal.Api.PayoutRecipientType.PAYPAL_ID,
                    receiver = recipientAccountId,
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
        catch (PaymentsException ex)
        {
            var errorResponse = ex.Response;
            _logger.LogError($"PayPal Payouts API Error: {errorResponse}");
            throw;
        }
        catch (PayPalException ex)
        {
            // Log general PayPal exceptions
            _logger.LogError(ex, "Error creating PayPal payout.");
            throw;
        }
    }
}

