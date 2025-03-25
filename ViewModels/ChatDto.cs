using System;
using System.Collections.Generic;

public class ChatDto
{
    public int Id { get; set; }
    public int ListingId { get; set; }
    public string TutorId { get; set; }
    public string StudentId { get; set; }
    public string RecipientId { get; set; }
    public string Name { get; set; }
    public string ProfileImagePath { get; set; }
    public string LastMessage { get; set; }
    public DateTime Timestamp { get; set; }
    public string Details { get; set; }
    public List<MessageDto> Messages { get; set; }
    public UserRole MyRole { get; set; }

    public ChatDto()
    {
        Name = string.Empty;
        ProfileImagePath = string.Empty;
        TutorId = string.Empty;
        StudentId = string.Empty;
        RecipientId = string.Empty;
        LastMessage = string.Empty;
        Details = string.Empty;
        Messages = new List<MessageDto>();
    }
}

public class MessageDto
{
    public string SentBy { get; set; } // "me" or "contact"
    public string SenderId { get; set; }
    public string SenderName { get; set; }
    public string Content { get; set; }
    public DateTime Timestamp { get; set; }

    public MessageDto()
    {
        SentBy = string.Empty;
        SenderId = string.Empty;
        SenderName = string.Empty;
        Content = string.Empty;
    }
}

public class SendMessageDto
{
    public int ListingId { get; set; }
    public string? RecipientId { get; set; }
    public string? Content { get; set; }

    public SendMessageDto()
    {
        Content = string.Empty;
    }
}

