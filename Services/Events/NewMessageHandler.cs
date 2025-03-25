using System;
using System.Threading.Tasks;
using Backend.Interfaces.Events;
using Backend.Services.Notifications.Messages;

namespace Backend.Services.Events
{
    public class NewMessageEvent
    {
        public int ChatId { get; set; }
        public string SenderId { get; set; } = string.Empty;
        public string RecipientId { get; set; } = string.Empty;
        public int ListingId { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string SenderName { get; set; } = string.Empty;
    }
    public class NewMessageHandler : IEventHandler<NewMessageEvent>
    {
        private readonly INotificationService _notificationService;

        public NewMessageHandler(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public async Task HandleAsync(NewMessageEvent eventData)
        {
            var emailSubject = "New Message Notification";
            var emailBody = $@"
            <p>Hi,</p>
            <p>You have received a new message from <strong>{eventData.SenderName}</strong>.</p>
            <p><strong>Message:</strong> {eventData.Content}</p>
            <p><strong>Sent At:</strong> {eventData.Timestamp:MMMM dd, yyyy HH:mm}</p>
            <p>Click <a href='https://www.avancira.com/messages'>here</a> to view and reply.</p>
            <p>Best regards,</p>
            <p>Avancira Team</p>";

            await _notificationService.NotifyAsync(
                eventData.RecipientId,
                NotificationEvent.NewMessage,
                $"You have a new message from {eventData.SenderName}",
                new NewMessage
                {
                    EmailSubject = emailSubject,
                    EmailBody = emailBody,
                    ChatId = eventData.ChatId,
                    SenderId = eventData.SenderId,
                    RecipientId = eventData.RecipientId,
                    ListingId = eventData.ListingId,
                    Content = eventData.Content,
                    Timestamp = eventData.Timestamp
                }
            );
        }
    }
}

