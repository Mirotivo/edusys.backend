using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Backend.Services
{
    public class TimerBackgroundService : BackgroundService
    {
        private readonly ILogger<TimerBackgroundService> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private Timer? _timer;

        public TimerBackgroundService(ILogger<TimerBackgroundService> logger, IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromHours(1));
            return Task.CompletedTask;
        }

        private void DoWork(object? state)
        {
            try
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var _paymentService = scope.ServiceProvider.GetRequiredService<IPaymentService>();
                    _logger.LogInformation("Processing past booked lessons.");
                    _paymentService.ProcessPastBookedLessons().GetAwaiter().GetResult();
                    _logger.LogInformation("Successfully processed past booked lessons.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing lessons: {ex.Message}");
            }
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return base.StopAsync(cancellationToken);
        }
    }
}
