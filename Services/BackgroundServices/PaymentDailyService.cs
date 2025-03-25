using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Backend.Services.PaymentBackgroundServices
{
    public class PaymentDailyService : BackgroundService
    {
        private readonly ILogger<PaymentDailyService> _logger;
        private readonly IServiceProvider _serviceProvider;

        public PaymentDailyService(ILogger<PaymentDailyService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("SubscriptionRenewalService is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Schedule next execution (midnight UTC)
                    var now = DateTime.UtcNow;
                    var nextRun = now.Date.AddDays(1);
                    var delay = nextRun - now;

                    _logger.LogInformation("Next subscription renewal check scheduled at {NextRun}.", nextRun);

                    await Task.Delay(delay, stoppingToken);

                    await ProcessSubscriptionRenewals(stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    _logger.LogInformation("SubscriptionRenewalService is stopping gracefully.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred in SubscriptionRenewalService.");
                }
            }
        }

        private async Task ProcessSubscriptionRenewals(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting subscription renewal processing at {Time}.", DateTime.UtcNow);

            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AvanciraDbContext>();
                var paymentService = scope.ServiceProvider.GetRequiredService<IPaymentService>();
                var subscriptionService = scope.ServiceProvider.GetRequiredService<ISubscriptionService>();
                var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                var today = DateTime.UtcNow.Date;

                // Fetch subscriptions due for renewal
                var subscriptionsToRenew = await dbContext.Subscriptions
                    .Include(s => s.User)
                    .Where(s => s.NextBillingDate.Date == today && s.CancellationDate == null)
                    .ToListAsync(stoppingToken);

                foreach (var subscription in subscriptionsToRenew)
                {
                    try
                    {
                        var user = subscription.User;
                        var paymentMethod = user.UserCards.FirstOrDefault(c => c.Type == UserCardType.Paying);

                        if (paymentMethod == null)
                        {
                            _logger.LogWarning("User {UserId} has no payment method. Subscription renewal failed.", user.Id);
                            continue;
                        }

                        var request = new SubscriptionRequestDto
                        {
                            BillingFrequency = subscription.BillingFrequency.ToString(),
                            PaymentMethod = TransactionPaymentMethod.Stripe,
                            PaymentType = TransactionPaymentType.StudentMembership, // Adjust based on actual type
                            Amount = subscription.Amount
                        };

                        // Reuse CreateSubscription method to handle renewal
                        var (subscriptionId, transactionId) = await subscriptionService.CreateSubscriptionAsync(request, user.Id);

                        _logger.LogInformation("Subscription {SubscriptionId} renewed for User {UserId}.", subscriptionId, user.Id);

                        // Notify User
                        await notificationService.NotifyAsync(
                            user.Id,
                            NotificationEvent.SubscriptionRenewed,
                            $"Your {subscription.BillingFrequency} subscription has been successfully renewed for {subscription.Amount:C} AUD.",
                            new { subscription.Amount, Currency = "AUD", subscription.NextBillingDate }
                        );
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing subscription renewal for Subscription {SubscriptionId}.", subscription.Id);
                    }
                }
            }

            _logger.LogInformation("Subscription renewal processing completed at {Time}.", DateTime.UtcNow);
        }
    }
}

