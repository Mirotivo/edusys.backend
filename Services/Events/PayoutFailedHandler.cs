using System;
using System.Threading.Tasks;
using Backend.Interfaces.Events;

namespace Backend.Services.Events
{
    public class PayoutFailedEvent
    {
        public string UserId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public DateTime AttemptedAt { get; set; }
    }
    public class PayoutFailedHandler : IEventHandler<PayoutFailedEvent>
    {
        private readonly INotificationService _notificationService;

        public PayoutFailedHandler(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public async Task HandleAsync(PayoutFailedEvent eventData)
        {
            var emailSubject = "Payout Processing Failed";
            var emailBody = $@"
            <p>Hi,</p>
            <p>Your payout of <strong>{eventData.Amount:C}</strong> failed to process.</p>
            <p><strong>Currency:</strong> {eventData.Currency}</p>
            <p><strong>Error Message:</strong> {eventData.ErrorMessage}</p>
            <p><strong>Attempted At:</strong> {eventData.AttemptedAt:MMMM dd, yyyy HH:mm}</p>
            <p>Please try again or contact support if the issue persists.</p>
            <p>Best regards,</p>
            <p>Avancira Team</p>";

            await _notificationService.NotifyAsync(
                eventData.UserId,
                NotificationEvent.PaymentFailed,
                $"Your payout of {eventData.Amount:C} failed to process. Please try again.",
                new
                {
                    Amount = eventData.Amount,
                    Currency = eventData.Currency,
                    ErrorMessage = eventData.ErrorMessage,
                    AttemptedAt = eventData.AttemptedAt
                }
            );
        }
    }
}

