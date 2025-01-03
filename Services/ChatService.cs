using Microsoft.EntityFrameworkCore;

public class ChatService : IChatService
{
    private readonly skillseekDbContext _dbContext;
    private readonly INotificationService _notificationService;

    public ChatService(skillseekDbContext dbContext, INotificationService notificationService)
    {
        _dbContext = dbContext;
        _notificationService = notificationService;
    }

    public List<ChatDto> GetChats(string userId)
    {
        var chats = _dbContext.Chats
            .Where(c => c.StudentId == userId || c.TutorId == userId)
            .Include(c => c.Listing != null ? c.Listing.LessonCategory : null)
            .Include(c => c.Messages)
            .ThenInclude(m => m.Sender)
            .ToList();

        var result = chats.Select(c =>
        {
            // Get the latest message
            var latestMessage = c.Messages
                .OrderByDescending(m => m.SentAt)
                .FirstOrDefault();

            return new ChatDto
            {
                Id = c.Id,
                ListingId = c.ListingId,
                TutorId = c.TutorId,
                StudentId = c.StudentId,
                RecipientId = c.StudentId == userId ? c.TutorId : c.StudentId,
                Details = c.StudentId == userId && c.Listing?.LessonCategory != null
                    ? $"{c.Listing.LessonCategory.Name} Tutor"
                    : c.Listing?.LessonCategory != null
                        ? $"{c.Listing.LessonCategory.Name} Student"
                        : "No lesson category",
                Name = c.StudentId == userId
                    ? c.Tutor?.FirstName ?? c.Tutor?.Email ?? "Unknown Tutor"
                    : c.Student?.FirstName ?? c.Student?.Email ?? "Unknown Student",
                LastMessage = latestMessage?.Content ?? "No messages yet",
                Timestamp = latestMessage?.SentAt.ToString("g") ?? string.Empty,
                Messages = c.Messages.Select(m => new MessageDto
                {
                    Text = m.Content,
                    SentBy = m.SenderId == userId ? "me" : "contact",
                    Timestamp = m.SentAt.ToString("g")
                }).ToList(),
                RequestDetails = c.StudentId == userId ? "Chat Request by Student" : "Chat Request by Tutor",
                MyRole = c.StudentId == userId ? "Student" : "Tutor"
            };
        }).ToList();
        return result;
    }

    public bool SendMessage(SendMessageDto messageDto, string senderId)
    {
        // Business logic to send messages
        var chat = _dbContext.Chats.FirstOrDefault(c =>
            (c.StudentId == senderId && c.TutorId == messageDto.RecipientId) ||
            (c.StudentId == messageDto.RecipientId && c.TutorId == senderId));

        if (chat == null)
        {
            chat = new Chat
            {
                StudentId = senderId,
                TutorId = messageDto.RecipientId ?? string.Empty,
                ListingId = messageDto.ListingId
            };
            _dbContext.Chats.Add(chat);
            _dbContext.SaveChanges();
        }

        var message = new Message
        {
            ChatId = chat.Id,
            SenderId = senderId,
            RecipientId = messageDto.RecipientId,
            Content = messageDto.Content ?? string.Empty,
            SentAt = DateTime.UtcNow,
            IsRead = false
        };

        _dbContext.Messages.Add(message);
        _dbContext.SaveChanges();


        // Retrieve sender's name
        var sender = _dbContext.Users.FirstOrDefault(u => u.Id == senderId);
        var senderName = sender?.FirstName ?? sender?.Email ?? "Someone";

        // Notify the recipient
        _notificationService.NotifyAsync(
            messageDto?.RecipientId,
            eventName: NotificationEvent.NewMessage,
            message: $"You have a new message from {senderName}",
            data: new
            {
                ChatId = chat.Id,
                SenderId = senderId,
                RecipientId = messageDto.RecipientId,
                Content = messageDto.Content,
                Timestamp = message.SentAt
            }
        );

        return true;
    }
}
