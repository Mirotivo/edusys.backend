using Backend.Database.Models;
using Backend.Services.Events;
using Backend.Services.Notifications.Messages;
using Microsoft.EntityFrameworkCore;

public class ChatService : IChatService
{
    private readonly AvanciraDbContext _dbContext;
    private readonly INotificationService _notificationService;
    private readonly ILogger<ChatService> _logger;

    public ChatService(AvanciraDbContext dbContext, INotificationService notificationService, ILogger<ChatService> logger)
    {
        _dbContext = dbContext;
        _notificationService = notificationService;
        _logger = logger;
    }

    public Chat GetOrCreateChat(string studentId, string tutorId, int listingId)
    {
        // Business logic to send messages
        var chat = _dbContext.Chats.FirstOrDefault(c => c.ListingId == listingId &&
            ((c.StudentId == studentId && c.TutorId == tutorId) || (c.StudentId == tutorId && c.TutorId == studentId)));

        if (chat == null)
        {
            chat = new Chat
            {
                StudentId = studentId,
                TutorId = tutorId,
                ListingId = listingId
            };
            _dbContext.Chats.Add(chat);
            _dbContext.SaveChanges();
        }

        return chat;
    }

    public List<ChatDto> GetUserChats(string userId)
    {
        var chats = _dbContext.Chats
            .Where(c => c.StudentId == userId || c.TutorId == userId)
            .Include(c => c.Listing != null ? c.Listing.LessonCategory : null)
            .Include(c => c.Tutor)
            .Include(c => c.Student)
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
                Details = c.Listing?.LessonCategory != null
                    ? $"{c.Listing.LessonCategory.Name} {(c.StudentId == userId ? "Tutor" : "Student")}"
                    : "No lesson category",
                Name = c.StudentId == userId
                    ? c.Tutor?.FullName ?? "Unknown Tutor"
                    : c.Student?.FullName ?? "Unknown Student",
                ProfileImagePath = c.StudentId == userId
                    ? c.Tutor?.ProfileImagePath ?? ""
                    : c.Student?.ProfileImagePath ?? "",
                LastMessage = latestMessage?.Content ?? "No messages yet",
                Timestamp = latestMessage?.SentAt.ToString("g") ?? string.Empty,
                Messages = c.Messages.Select(m => new MessageDto
                {
                    Text = m.Content,
                    SentBy = m.SenderId == userId ? "me" : "contact",
                    Timestamp = m.SentAt.ToString("g")
                }).ToList(),
                RequestDetails = c.StudentId == userId ? "Chat Request by Student" : "Chat Request by Tutor",
                MyRole = c.StudentId == userId ? Role.Student : Role.Tutor
            };
        }).ToList();
        return result;
    }

    public bool SendMessage(SendMessageDto messageDto, string senderId)
    {
        var chat = GetOrCreateChat(senderId, messageDto.RecipientId ?? string.Empty, messageDto.ListingId);

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
        var senderName = sender?.FullName ?? "Someone";
        var eventData = new NewMessageEvent
        {
            ChatId = chat.Id,
            SenderId = senderId,
            RecipientId = messageDto.RecipientId,
            ListingId = messageDto.ListingId,
            Content = messageDto.Content,
            Timestamp = message.SentAt,
            SenderName = senderName
        };

        _notificationService.NotifyAsync(NotificationEvent.NewMessage, eventData);
        return true;
    }
}
