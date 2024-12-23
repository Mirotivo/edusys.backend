using Microsoft.EntityFrameworkCore;

public class SubscriptionService : ISubscriptionService
{
    private readonly skillseekDbContext _dbContext;
    private readonly IPaymentService _paymentService;

    public SubscriptionService(
        skillseekDbContext dbContext,
        IPaymentService paymentService
    )
    {
        _dbContext = dbContext;
        _paymentService = paymentService;
    }

    public async Task<bool> CheckActiveSubscription(int userId)
    {
        return await _dbContext.Subscriptions
            .AnyAsync(s => s.UserId == userId && s.Status == SubscriptionStatus.Active && s.EndDate > DateTime.UtcNow);
    }

    public async Task<(int SubscriptionId, int TransactionId)> CreateSubscription(SubscriptionRequestDto request, int userId)
    {
        var user = await _dbContext.Users.FindAsync(userId);
        if (user == null) throw new KeyNotFoundException("User not found.");

        PaymentMethod paymentMethod = PaymentMethod.Stripe;
        if (!string.IsNullOrEmpty(request.PaymentMethod) &&
            !Enum.TryParse(request.PaymentMethod, true, out paymentMethod))
        {
            paymentMethod = PaymentMethod.Stripe;
        }

        // Use shared transaction processing logic
        var transaction = await _paymentService.ProcessTransactionAsync(
            stripeCustomerId: user.StripeCustomerId,
            senderId: userId,
            recipientId: null, // Platform
            amount: request.Amount,
            paymentType: request.SubscriptionType == SubscriptionType.Student ? PaymentType.StudentMembership : PaymentType.TutorMembership,
            gatewayName: paymentMethod.ToString()
        );

        // Create the subscription
        var subscription = new Subscription
        {
            UserId = userId,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(1),
            Amount = request.Amount,
            Status = SubscriptionStatus.Active,
            Plan = request.SubscriptionType == SubscriptionType.Student ? "Student Pass" : "Tutor Premium"
        };

        await _dbContext.Subscriptions.AddAsync(subscription);
        await _dbContext.SaveChangesAsync();

        return (subscription.Id, transaction.Id);
    }

    public async Task<List<Subscription>> GetUserSubscriptions(int userId)
    {
        return await _dbContext.Subscriptions
            .Where(s => s.UserId == userId)
            .ToListAsync();
    }
}
