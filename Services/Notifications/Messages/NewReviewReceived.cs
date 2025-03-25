using System;

namespace Backend.Services.Notifications.Messages
{
    public class NewReviewReceived : IEmailNotificationData
    {
        public string EmailSubject { get; set; } = string.Empty;
        public string EmailBody { get; set; } = string.Empty;
        public int ReviewId { get; set; }
        public string ReviewerId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Comments { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}

