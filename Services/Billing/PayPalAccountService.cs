using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Backend.Interfaces.Billing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Backend.Services.Billing
{

    public class PayPalTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }

        [JsonPropertyName("token_type")]
        public string? TokenType { get; set; }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonPropertyName("refresh_token")]
        public string? RefreshToken { get; set; }

        [JsonPropertyName("payer_id")]
        public string? PayerId { get; set; }
    }

    public class PayPalAccountService : IPayPalAccountService
    {
        private readonly HttpClient _httpClient;
        private readonly AppOptions _appOptions;
        private readonly PayPalOptions _payPalOptions;
        private readonly AvanciraDbContext _dbContext;
        private readonly ILogger<PayPalAccountService> _logger;

        public PayPalAccountService(
            HttpClient httpClient,
            Microsoft.Extensions.Options.IOptions<AppOptions> appOptions,
            Microsoft.Extensions.Options.IOptions<PayPalOptions> payPalOptions,
            AvanciraDbContext dbContext,
            ILogger<PayPalAccountService> logger
        )
        {
            _httpClient = httpClient;
            _appOptions = appOptions.Value;
            _payPalOptions = payPalOptions.Value;
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<bool> ConnectPayPalAccountAsync(string userId, string authCode)
        {
            var clientId = _payPalOptions.ClientId;
            var clientSecret = _payPalOptions.ClientSecret;
            var redirectUri = $"{_appOptions.FrontEndUrl}/dashboard/payments?section=payments&detail=receiving";


            // #1 Step 1: Exchange Authorization Code for PayPal Access Token & Payer ID
            var authString = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{clientId}:{clientSecret}"));
            var api = _payPalOptions.Environment.ToLower() switch
            {
                "sandbox" => "https://api.sandbox.paypal.com/v1/identity/oauth2/userinfo?schema=paypalv1.1",
                "live" => "https://api.paypal.com/v1/identity/oauth2/userinfo?schema=paypalv1.1",
                _ => throw new InvalidOperationException("Invalid PayPal environment configuration. Use 'Sandbox' or 'Live'.")
            };
            var request = new HttpRequestMessage(HttpMethod.Get, api)
            {
                Headers = { Authorization = new AuthenticationHeaderValue("Basic", authString) }
            };
            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode) return false;
            var tokenData = JsonSerializer.Deserialize<PayPalTokenResponse>(await response.Content.ReadAsStringAsync());
            var payerId = tokenData?.PayerId;

            if (string.IsNullOrEmpty(payerId)) return false;

            // #2 Step 2: Store Payer ID in the Database
            var user = await _dbContext.Users.AsTracking().FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) throw new Exception("User not found.");

            user.PayPalAccountId = payerId;
            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
}

