using System;

namespace Backend.Services.Notifications.Messages
{
    public class PayoutProcessed : IEmailNotificationData
    {
        public string EmailSubject { get; set; } = string.Empty;
        public string EmailBody { get; set; } = string.Empty;
        public decimal Amount { get; internal set; }
        public string Currency { get; internal set; } = string.Empty;
        public decimal WalletBalance { get; internal set; }
        public DateTime ProcessedAt { get; internal set; }
    }
}

