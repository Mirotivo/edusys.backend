using Backend.Interfaces.Billing;
using Stripe;

namespace Backend.Services.Billing
{
    public class StripeCardService : IStripeCardService
    {
        public async Task<Card> CreateAsync(string customerId, CardCreateOptions options)
        {
            var cardService = new CardService();
            return await cardService.CreateAsync(customerId, options);
        }
    }
}
