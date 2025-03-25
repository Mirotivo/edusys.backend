using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Message
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int ChatId { get; set; }

    [ForeignKey(nameof(Message.ChatId))]
    public Chat? Chat { get; set; }

    [Required]
    public string SenderId { get; set; }

    [ForeignKey(nameof(Message.SenderId))]
    public User? Sender { get; set; }

    public string? RecipientId { get; set; }

    [ForeignKey(nameof(Message.RecipientId))]
    public User? Recipient { get; set; }

    [Required]
    [MaxLength(1000)]
    public string Content { get; set; }

    public DateTime SentAt { get; set; }

    public bool IsRead { get; set; }

    public Message()
    {
        SenderId = string.Empty;
        Content = string.Empty;
        SentAt = DateTime.UtcNow;
        IsRead = false;
    }

    public override string ToString()
    {
        return $"Message: {Id}, ChatId: {ChatId}, SenderId: {SenderId}, Content: {Content}, SentAt: {SentAt}, IsRead: {IsRead}";
    }
}

