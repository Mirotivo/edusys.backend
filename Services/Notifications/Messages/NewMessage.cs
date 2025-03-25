using System;

namespace Backend.Services.Notifications.Messages
{
    public class NewMessage : IEmailNotificationData
    {
        public string EmailSubject { get; set; } = string.Empty;
        public string EmailBody { get; set; } = string.Empty;
        public int ChatId { get; set; }
        public string SenderId { get; set; } = string.Empty;
        public string? RecipientId { get; set; }
        public int ListingId { get; set; }
        public string? Content { get; set; }
        public DateTime Timestamp { get; set; }
    }
}

