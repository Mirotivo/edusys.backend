public interface ISubscriptionService
{
    Task<bool> CheckActiveSubscription(string userId);
    Task<(int SubscriptionId, int TransactionId)> CreateSubscription(SubscriptionRequestDto request, string userId);
    Task<List<Subscription>> GetUserSubscriptions(string userId);
}
