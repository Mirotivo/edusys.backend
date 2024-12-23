public interface ISubscriptionService
{
    Task<bool> CheckActiveSubscription(int userId);
    Task<(int SubscriptionId, int TransactionId)> CreateSubscription(SubscriptionRequestDto request, int userId);
    Task<List<Subscription>> GetUserSubscriptions(int userId);
}
