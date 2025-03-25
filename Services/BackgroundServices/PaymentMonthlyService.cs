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
    public class PaymentMonthlyService : BackgroundService
    {
        private readonly ILogger<PaymentMonthlyService> _logger;
        private readonly IServiceProvider _serviceProvider;

        public PaymentMonthlyService(ILogger<PaymentMonthlyService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("MonthlyPaymentService is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Calculate the next execution time (1st day of the next month at 00:00 UTC)
                    var now = DateTime.UtcNow;
                    var nextRun = new DateTime(now.Year, now.Month, 1, 0, 0, 0).AddMonths(1);

                    var delay = nextRun - now;

                    _logger.LogInformation("Next monthly payment processing scheduled at {NextRun}.", nextRun);

                    // Wait until the next execution time or cancellation
                    await Task.Delay(delay, stoppingToken);

                    // Execute payment processing logic
                    await ProcessMonthlyPayments(stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    _logger.LogInformation("MonthlyPaymentService is stopping gracefully.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred in MonthlyPaymentService.");
                }
            }
        }

        private async Task ProcessMonthlyPayments(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting monthly payment processing at {Time}.", DateTime.UtcNow);

            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AvanciraDbContext>();
                var paymentService = scope.ServiceProvider.GetRequiredService<IPaymentService>();
                var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                // Fetch tutors with a Monthly payment preference and a positive wallet balance
                var tutorsWithWallets = await dbContext.Wallets
                    .Include(w => w.User) // Join with User
                    .Where(w => w.User.PaymentSchedule == UserPaymentSchedule.Monthly && w.Balance > 0)
                    .ToListAsync(stoppingToken);

                foreach (var wallet in tutorsWithWallets)
                {
                    try
                    {
                        var tutor = wallet.User;
                        var payoutAmount = wallet.Balance;

                        // Create the payout
                        var payoutResult = await paymentService.CreatePayoutAsync(tutor.Id, payoutAmount, "AUD", tutor.PaymentGateway);

                        _logger.LogInformation("Payout of {Amount} processed for Tutor {TutorId}.", payoutAmount, tutor.Id);

                        // Notify the tutor about the payout
                        await notificationService.NotifyAsync(
                            tutor.Id,
                            NotificationEvent.PayoutProcessed,
                            $"Your monthly payout of {payoutAmount:C} has been processed successfully.",
                            new
                            {
                                Amount = payoutAmount,
                                Currency = "AUD",
                                WalletBalance = wallet.Balance,
                                ProcessedAt = DateTime.UtcNow
                            }
                        );
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to process payout for Tutor {TutorId}.", wallet.UserId);
                    }
                }
            }

            _logger.LogInformation("Monthly payment processing completed at {Time}.", DateTime.UtcNow);
        }
    }
}

