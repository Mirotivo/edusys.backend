using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Chat : ICreatable
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int StudentId { get; set; }

    [ForeignKey(nameof(Chat.StudentId))]
    public User? Student { get; set; }

    [Required]
    public int TutorId { get; set; }

    [ForeignKey(nameof(Chat.TutorId))]
    public User? Tutor { get; set; }

    [Required]
    public int ListingId { get; set; }

    [ForeignKey(nameof(Chat.ListingId))]
    public Listing? Listing { get; set; }

    public DateTime CreatedAt { get; set; }

    public List<Message> Messages { get; set; }

    public Chat()
    {
        Messages = new List<Message>();
    }
    public override string ToString()
    {
        return $"Chat: {Id}, StudentId: {StudentId}, TutorId: {TutorId}, ListingId: {ListingId}, CreatedAt: {CreatedAt}";
    }
}
