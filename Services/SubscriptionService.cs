using Microsoft.EntityFrameworkCore;

public class SubscriptionService : ISubscriptionService
{
    private readonly AvanciraDbContext _dbContext;
    private readonly IPaymentService _paymentService;
    private readonly ILogger<SubscriptionService> _logger;

    public SubscriptionService(
        AvanciraDbContext dbContext,
        IPaymentService paymentService,
        ILogger<SubscriptionService> logger
    )
    {
        _dbContext = dbContext;
        _paymentService = paymentService;
        _logger = logger;
    }

    public async Task<bool> CheckActiveSubscription(string userId)
    {
        return await _dbContext.Subscriptions
            .AnyAsync(s => s.UserId == userId && s.Status == SubscriptionStatus.Active && s.EndDate > DateTime.UtcNow);
    }

    public async Task<(int SubscriptionId, int TransactionId)> CreateSubscription(SubscriptionRequestDto request, string userId)
    {
        var user = await _dbContext.Users.FindAsync(userId);
        if (user == null) throw new KeyNotFoundException("User not found.");

        if (string.IsNullOrEmpty(user.StripeCustomerId))
        {
            throw new InvalidOperationException("StripeCustomerId is missing. Please ensure the user has a Stripe account.");
        }

        var paymentMethod = PaymentMethod.Stripe;
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

    public async Task<List<Subscription>> GetUserSubscriptions(string userId)
    {
        return await _dbContext.Subscriptions
            .Where(s => s.UserId == userId)
            .ToListAsync();
    }
}
