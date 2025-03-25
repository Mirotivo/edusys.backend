namespace Backend.Services.Notifications.Messages
{
    public interface IEmailNotificationData
    {
        string EmailSubject { get; }
        string EmailBody { get; }
    }
}

