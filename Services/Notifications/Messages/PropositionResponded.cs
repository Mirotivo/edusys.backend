namespace Backend.Services.Notifications.Messages
{
    public class PropositionResponded : IEmailNotificationData
    {
        public string EmailSubject { get; set; } = string.Empty;
        public string EmailBody { get; set; } = string.Empty;
        public int LessonId { get; internal set; }
        public LessonStatus Status { get; internal set; }
    }
}

