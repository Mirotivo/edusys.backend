namespace Backend.Services.Notifications.Messages
{
    public class ConfirmEmail : IEmailNotificationData
    {
        public string EmailSubject { get; set; } = string.Empty;
        public string EmailBody { get; set; } = string.Empty;
        public string ConfirmationLink { get; set; } = string.Empty;
    }
}

