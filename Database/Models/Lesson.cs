using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


public class Lesson : ICreatable, IUpdatable
{
    [Key]
    public int Id { get; set; }

    [Required]
    public DateTime Date { get; set; }

    [Required]
    public TimeSpan Duration { get; set; }

    [Required]
    public decimal Price { get; set; }

    public string StudentId { get; set; }

    [ForeignKey(nameof(Lesson.StudentId))]
    public User? Student { get; set; }

    public int ListingId { get; set; }

    [ForeignKey(nameof(Lesson.ListingId))]
    public Listing? Listing { get; set; }

    [Required]
    public bool IsStudentInitiated { get; set; }

    public int TransactionId { get; set; }

    [ForeignKey(nameof(Lesson.TransactionId))]
    public Transaction? Transaction { get; set; }

    [Required]
    [MaxLength(20)]
    public LessonStatus Status { get; set; }

    public string? MeetingToken { get; set; }
    public string? MeetingRoomName { get; set; }
    public string? MeetingDomain { get; set; }
    public string? MeetingUrl { get; set; }
    public string? MeetingRoomUrl { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Lesson()
    {
        StudentId = string.Empty;
    }

    public override string ToString()
    {
        return $"Lesson: {Id}, StudentId: {StudentId}, ListingId: {ListingId}, Date: {Date}, Duration: {Duration}, Price: {Price:C}, Status: {Status}";
    }
}

