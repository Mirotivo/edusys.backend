using Microsoft.AspNetCore.SignalR;

public enum NotificationEvent
{
    // Authentication-related events
    ChangePassword,
    ForgotPassword,
    LoginAttempt,

    // Message and chat-related events
    NewMessage,
    ChatRequest,
    MessageRead,

    // Transaction and payment-related events
    PayoutProcessed,
    PaymentFailed,
    PaymentReceived,
    RefundProcessed,

    // Booking or scheduling-related events
    BookingConfirmed,
    BookingCancelled,
    BookingReminder,

    // Review-related events
    NewReviewReceived,

    // General notifications
    ProfileUpdated,
    SystemAlert,
    NewFeatureAnnouncement,
}

public class NotificationService : INotificationService
{
    private readonly IEnumerable<INotificationChannel> _channels;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        IEnumerable<INotificationChannel> channels,
        ILogger<NotificationService> logger
    )
    {
        _channels = channels;
        _logger = logger;
    }

    public async Task NotifyAsync(string userId, NotificationEvent eventName, string message, object? data = null)
    {
        var notification = new Notification
        {
            EventName = eventName,
            Message = message,
            Data = data
        };

        foreach (var channel in _channels)
        {
            try
            {
                await channel.SendAsync(userId, notification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification via {ChannelType} to User ID: {UserId}. Event: {EventName}", channel.GetType().Name, userId, eventName);
            }
        }
    }
}
