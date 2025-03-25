using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Backend.Database.Models;
using Microsoft.Extensions.Logging;


public class NotificationService : INotificationService
{
    private readonly AvanciraDbContext _dbContext;
    private readonly IEnumerable<INotificationChannel> _channels;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        AvanciraDbContext dbContext,
        IEnumerable<INotificationChannel> channels,
        ILogger<NotificationService> logger
    )
    {
        _dbContext = dbContext;
        _channels = channels;
        _logger = logger;
    }

    public async Task NotifyAsync<T>(NotificationEvent eventType, T eventData)
    {
        var storedEvent = new StoredEvent
        {
            EventType = eventType,
            Data = JsonSerializer.Serialize(eventData)
        };

        _dbContext.Events.Add(storedEvent);
        await _dbContext.SaveChangesAsync();
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

