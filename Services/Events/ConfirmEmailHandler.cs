using System.Threading.Tasks;
using Backend.Interfaces.Events;
using Backend.Services.Notifications.Messages;

namespace Backend.Services.Events
{
    public class ConfirmEmailEvent
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string ConfirmationLink { get; set; } = string.Empty;
    }
    public class ConfirmEmailHandler : IEventHandler<ConfirmEmailEvent>
    {
        private readonly INotificationService _notificationService;

        public ConfirmEmailHandler(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public async Task HandleAsync(ConfirmEmailEvent eventData)
        {
            var emailSubject = "Email Confirmation Required";
            var emailBody = $@"
            <p>Hi,</p>
            <p>Please confirm your email address by clicking <a href='{eventData.ConfirmationLink}'>here</a>.</p>
            <p>Best regards,</p>
            <p>Avancira</p>";

            await _notificationService.NotifyAsync(
                eventData.UserId,
                NotificationEvent.ConfirmEmail,
                "Email Confirmation Required",
                new ConfirmEmail
                {
                    EmailSubject = emailSubject,
                    EmailBody = emailBody,
                    ConfirmationLink = eventData.ConfirmationLink
                }
            );
        }
    }
}

