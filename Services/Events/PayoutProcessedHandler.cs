using System;
using System.Threading.Tasks;
using Backend.Interfaces.Events;
using Backend.Services.Notifications.Messages;

namespace Backend.Services.Events
{
    public class PayoutProcessedEvent
    {
        public string UserId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public decimal WalletBalance { get; set; }
        public DateTime ProcessedAt { get; set; }
    }
    public class PayoutProcessedHandler : IEventHandler<PayoutProcessedEvent>
    {
        private readonly INotificationService _notificationService;

        public PayoutProcessedHandler(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public async Task HandleAsync(PayoutProcessedEvent eventData)
        {
            var emailSubject = "Payout Processed Successfully";
            var emailBody = $@"
            <p>Hi,</p>
            <p>Your payout of <strong>{eventData.Amount:C}</strong> has been successfully processed.</p>
            <p><strong>Currency:</strong> {eventData.Currency}</p>
            <p><strong>Remaining Wallet Balance:</strong> {eventData.WalletBalance:C}</p>
            <p><strong>Processed At:</strong> {eventData.ProcessedAt:MMMM dd, yyyy HH:mm}</p>
            <p>Best regards,</p>
            <p>Avancira Team</p>";

            await _notificationService.NotifyAsync(
                eventData.UserId,
                NotificationEvent.PayoutProcessed,
                $"Your payout of {eventData.Amount:C} has been successfully processed.",
                new PayoutProcessed
                {
                    EmailSubject = emailSubject,
                    EmailBody = emailBody,
                    Amount = eventData.Amount,
                    Currency = eventData.Currency,
                    WalletBalance = eventData.WalletBalance,
                    ProcessedAt = eventData.ProcessedAt
                }
            );
        }
    }
}

