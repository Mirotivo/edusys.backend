using System;

namespace Backend.Services.Notifications.Messages
{
    public class NewRecommendationReceived : IEmailNotificationData
    {
        public string EmailSubject { get; set; } = string.Empty;
        public string EmailBody { get; set; } = string.Empty;
        public int RecommendationId { get; set; }
        public string ReviewerId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Comments { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}

