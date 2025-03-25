using System;
using System.Threading.Tasks;
using Backend.Interfaces.Events;
using Backend.Services.Notifications.Messages;

namespace Backend.Services.Events
{
    public class NewRecommendationReceivedEvent
    {
        public int RecommendationId { get; set; }
        public string ReviewerId { get; set; } = string.Empty;
        public string RevieweeId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Comments { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string ReviewerName { get; set; } = string.Empty;
    }
    public class NewRecommendationReceivedHandler : IEventHandler<NewRecommendationReceivedEvent>
    {
        private readonly INotificationService _notificationService;

        public NewRecommendationReceivedHandler(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public async Task HandleAsync(NewRecommendationReceivedEvent eventData)
        {
            var emailSubject = "New Recommendation Received";
            var emailBody = $@"
            <p>Hi,</p>
            <p>You have received a new recommendation from <strong>{eventData.ReviewerName}</strong>.</p>
            <p><strong>Recommendation Title:</strong> {eventData.Title}</p>
            <p><strong>Feedback:</strong> {eventData.Comments}</p>
            <p><strong>Submitted At:</strong> {eventData.Timestamp:MMMM dd, yyyy HH:mm}</p>
            <p>Click <a href='https://www.avancira.com/reviews'>here</a> to view your recommendation.</p>
            <p>Best regards,</p>
            <p>Avancira Team</p>";

            await _notificationService.NotifyAsync(
                eventData.RevieweeId,
                NotificationEvent.NewRecommendationReceived,
                $"You have received a new recommendation from {eventData.ReviewerName}.",
                new NewRecommendationReceived
                {
                    EmailSubject = emailSubject,
                    EmailBody = emailBody,
                    RecommendationId = eventData.RecommendationId,
                    ReviewerId = eventData.ReviewerId,
                    Title = eventData.Title,
                    Comments = eventData.Comments,
                    Timestamp = eventData.Timestamp
                }
            );
        }
    }
}

