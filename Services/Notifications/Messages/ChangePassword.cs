using System;

namespace Backend.Services.Notifications.Messages
{
    public class ChangePassword : IEmailNotificationData
    {
        public string EmailSubject { get; set; } = string.Empty;
        public string EmailBody { get; set; } = string.Empty;
        public string ResetLink { get; set; } = string.Empty;
        public DateTime ExpiryDate { get; set; }
    }
}

