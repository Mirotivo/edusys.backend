using System;

namespace Backend.Services.Notifications.Messages
{
    public class BookingRequested : IEmailNotificationData
    {
        public string EmailSubject { get; set; } = string.Empty;
        public string EmailBody { get; set; } = string.Empty;
        public int LessonId { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan Duration { get; set; }
        public decimal Price { get; set; }
    }
}

