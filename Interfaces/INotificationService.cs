public interface INotificationService
{
    Task NotifyAsync(string userId, NotificationEvent eventName, string message, object? data = null);
}
