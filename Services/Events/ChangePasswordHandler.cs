using System;
using System.Threading.Tasks;
using Backend.Interfaces.Events;
using Backend.Services.Notifications.Messages;

namespace Backend.Services.Events
{
    public class ChangePasswordEvent
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string ResetLink { get; set; } = string.Empty;
        public DateTime ExpiryDate { get; set; }
    }
    public class ChangePasswordHandler : IEventHandler<ChangePasswordEvent>
    {
        private readonly INotificationService _notificationService;

        public ChangePasswordHandler(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public async Task HandleAsync(ChangePasswordEvent eventData)
        {
            var emailSubject = "Password Reset Request";
            var emailBody = $@"
            <p>Hi,</p>
            <p>Click <a href='{eventData.ResetLink}'>here</a> to reset your password.</p>
            <p>The link will expire on <strong>{eventData.ExpiryDate:MMMM dd, yyyy HH:mm} UTC</strong>.</p>
            <p>If you didn't request a password reset, you can ignore this email.</p>
            <p>Best regards,</p>
            <p>Avancira Team</p>";

            await _notificationService.NotifyAsync(
                eventData.UserId,
                NotificationEvent.ChangePassword,
                "Password Reset Request",
                new ChangePassword
                {
                    EmailSubject = emailSubject,
                    EmailBody = emailBody,
                    ResetLink = eventData.ResetLink,
                    ExpiryDate = eventData.ExpiryDate
                }
            );
        }
    }
}

