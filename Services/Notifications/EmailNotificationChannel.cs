using System.Linq;
using System.Threading.Tasks;
using Backend.Services.Notifications.Messages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public class EmailNotificationChannel : INotificationChannel
{
    private readonly IEmailService _emailService;
    private readonly AvanciraDbContext _dbContext;
    private readonly ILogger<EmailNotificationChannel> _logger;

    public EmailNotificationChannel(
        IEmailService emailService,
        AvanciraDbContext dbContext,
        ILogger<EmailNotificationChannel> logger
    )
    {
        _emailService = emailService;
        _dbContext = dbContext;
        _logger = logger;
    }


    public async Task SendAsync(string userId, Notification notification)
    {
        // Fetch email from the User table
        var email = await _dbContext.Users
            .Where(u => u.Id == userId)
            .Select(u => u.Email)
            .FirstOrDefaultAsync();

        var data = notification.Data as IEmailNotificationData;
        if (!string.IsNullOrEmpty(email))
        {
            var subject = !string.IsNullOrEmpty(data?.EmailSubject) == true
                ? data.EmailSubject?.ToString()
                : $"Notification: {notification.EventName}";
            var body = !string.IsNullOrEmpty(data?.EmailBody) == true
                ? data.EmailBody?.ToString()
                : notification.Message;

            await _emailService.SendEmailAsync(email, subject ?? string.Empty, body ?? string.Empty);
            _logger.LogInformation("Email sent successfully to {Email}. Subject: {Subject}", email, subject);
        }
        else
        {
            _logger.LogWarning("Email address not found for user {UserId}. Notification: {EventName}", userId, notification.EventName);
        }
    }
}

