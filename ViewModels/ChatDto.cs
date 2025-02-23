public class ChatDto
{
    public int Id { get; set; }
    public int ListingId { get; set; }
    public string TutorId { get; set; }
    public string StudentId { get; set; }
    public string RecipientId { get; set; }
    public string Name { get; set; }
    public string LastMessage { get; set; }
    public string Timestamp { get; set; }
    public string Details { get; set; }
    public List<MessageDto> Messages { get; set; }
    public string RequestDetails { get; set; }
    public Role MyRole { get; set; }

    public ChatDto()
    {
        Name = string.Empty;
        TutorId = string.Empty;
        StudentId = string.Empty;
        RecipientId = string.Empty;
        LastMessage = string.Empty;
        Timestamp = string.Empty;
        Details = string.Empty;
        RequestDetails = string.Empty;
        Messages = new List<MessageDto>();
    }
}

public class MessageDto
{
    public string Text { get; set; }
    public string SentBy { get; set; } // "me" or "contact"
    public string Timestamp { get; set; }

    public MessageDto()
    {
        Text = string.Empty;
        SentBy = string.Empty;
        Timestamp = string.Empty;
    }
}

public class SendMessageDto
{
    public int ListingId { get; set; } // The ID of the message recipient
    public string? RecipientId { get; set; } // The ID of the message recipient
    public string? Content { get; set; } // The message text

    public SendMessageDto()
    {
        Content = string.Empty;
    }
}
