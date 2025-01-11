using Stripe;

namespace Backend.Interfaces.Billing
{
    public interface IStripeCardService
    {
        Task<Card> CreateAsync(string customerId, CardCreateOptions options);
    }
}
