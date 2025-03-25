using System;
using System.Threading.Tasks;
using Backend.Interfaces.Events;
using Backend.Services.Notifications.Messages;

namespace Backend.Services.Events
{
    public class NewReviewReceivedEvent
    {
        public int ReviewId { get; set; }
        public string ReviewerId { get; set; } = string.Empty;
        public string RevieweeId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Comments { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string ReviewerName { get; set; } = string.Empty;
    }
    public class NewReviewReceivedHandler : IEventHandler<NewReviewReceivedEvent>
    {
        private readonly INotificationService _notificationService;

        public NewReviewReceivedHandler(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public async Task HandleAsync(NewReviewReceivedEvent eventData)
        {
            var emailSubject = "New Review Received";
            var emailBody = $@"
            <p>Hi,</p>
            <p>You have received a new review from <strong>{eventData.ReviewerName}</strong>.</p>
            <p><strong>Review Title:</strong> {eventData.Title}</p>
            <p><strong>Feedback:</strong> {eventData.Comments}</p>
            <p><strong>Submitted At:</strong> {eventData.Timestamp:MMMM dd, yyyy HH:mm}</p>
            <p>Click <a href='https://www.avancira.com/reviews'>here</a> to view your review.</p>
            <p>Best regards,</p>
            <p>Avancira Team</p>";

            await _notificationService.NotifyAsync(
                eventData.RevieweeId,
                NotificationEvent.NewReviewReceived,
                $"You have received a new review from {eventData.ReviewerName}.",
                new NewReviewReceived
                {
                    EmailSubject = emailSubject,
                    EmailBody = emailBody,
                    ReviewId = eventData.ReviewId,
                    ReviewerId = eventData.ReviewerId,
                    Title = eventData.Title,
                    Comments = eventData.Comments,
                    Timestamp = eventData.Timestamp
                }
            );
        }
    }
}

