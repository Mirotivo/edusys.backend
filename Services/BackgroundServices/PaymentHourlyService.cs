using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Backend.Services.PaymentBackgroundServices
{
    public class PaymentHourlyService : BackgroundService
    {
        private readonly ILogger<PaymentHourlyService> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private Timer? _timer;

        public PaymentHourlyService(ILogger<PaymentHourlyService> logger, IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromMinutes(60));
            return Task.CompletedTask;
        }

        private void DoWork(object? state)
        {
            try
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var _lessonService = scope.ServiceProvider.GetRequiredService<ILessonService>();
                    _logger.LogInformation("Processing past booked lessons.");
                    _lessonService.ProcessPastBookedLessons().GetAwaiter().GetResult();
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

