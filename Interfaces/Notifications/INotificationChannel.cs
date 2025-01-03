public interface INotificationChannel
{
    Task SendAsync(string userId, Notification notification);
}
