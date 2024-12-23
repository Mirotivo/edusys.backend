using Microsoft.EntityFrameworkCore;

public class SmsNotificationChannel : INotificationChannel
{
    private readonly ISmsService _smsService;
    private readonly skillseekDbContext _dbContext;
    private readonly ILogger<SmsNotificationChannel> _logger;

    public SmsNotificationChannel(
        ISmsService smsService,
        skillseekDbContext dbContext,
        ILogger<SmsNotificationChannel> logger
    )
    {
        _smsService = smsService;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task SendAsync(string userId, Notification notification)
    {
        // Fetch phone number from the User table
        var phoneNumber = await _dbContext.Users
            .Where(u => u.Id.ToString() == userId)
            .Select(u => u.Phone)
            .FirstOrDefaultAsync();

        if (!string.IsNullOrEmpty(phoneNumber))
        {
            try
            {
                var message = notification.Message;
                await _smsService.SendSmsAsync(phoneNumber, message);

                _logger.LogInformation("SMS sent to {PhoneNumber}. Message: {Message}", phoneNumber, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send SMS to {PhoneNumber} for user {UserId}", phoneNumber, userId);
                throw; // Re-throw exception if necessary
            }
        }
        else
        {
            _logger.LogWarning("SMS: Phone number not found for user {UserId}.", userId);
        }
    }
}
