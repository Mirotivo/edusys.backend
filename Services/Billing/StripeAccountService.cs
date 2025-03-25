using System.Collections.Generic;
using System.Threading.Tasks;
using Backend.Interfaces.Billing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Stripe;

namespace Backend.Services.Billing
{
    public class StripeAccountService : IStripeAccountService
    {
        private readonly AppOptions _appOptions;
        private readonly StripeOptions _stripeOptions;
        private readonly AvanciraDbContext _dbContext;
        private readonly ILogger<StripeAccountService> _logger;

        public StripeAccountService(
            Microsoft.Extensions.Options.IOptions<AppOptions> appOptions,
            Microsoft.Extensions.Options.IOptions<StripeOptions> stripeOptions,
            AvanciraDbContext dbContext,
            ILogger<StripeAccountService> logger
        )
        {
            _appOptions = appOptions.Value;
            _stripeOptions = stripeOptions.Value;
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<string> ConnectStripeAccountAsync(string userId)
        {
            var user = await _dbContext.Users.AsTracking().FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                throw new KeyNotFoundException("User not found.");

            var accountId = await CreateStripeAccountAsync(user.Email);

            // Save the Account ID (account.Id) in your database
            user.StripeConnectedAccountId = accountId;
            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync();

            return await CreateOnboardingLinkAsync(accountId);
        }

        private async Task<string> CreateOnboardingLinkAsync(string accountId)
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

        private async Task<string> CreateStripeAccountAsync(string email)
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
    }
}

