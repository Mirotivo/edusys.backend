using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

public class SignalRNotificationChannel : INotificationChannel
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<SignalRNotificationChannel> _logger;

    public SignalRNotificationChannel(
        IHubContext<NotificationHub> hubContext,
        ILogger<SignalRNotificationChannel> logger
    )
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task SendAsync(string userId, Notification notification)
    {
        var connectionId = NotificationHub.GetConnectionId(userId);
        if (!string.IsNullOrEmpty(connectionId))
        {
            await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveNotification", notification);
        }
        else
        {
            _logger.LogWarning("SignalR: User {UserId} is not connected.", userId);
        }
    }
}

