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

    [Required]
    public string StudentId { get; set; } = string.Empty;

    [Required]
    public int ListingId { get; set; }

    [Required]
    public int TransactionId { get; set; }

    [Required]
    public bool IsStudentInitiated { get; set; }

    [Required]
    public LessonStatus Status { get; set; }

    // TODO: Remove Domain , check if Url and RoomUrl are needed
    public string? MeetingToken { get; set; }
    public string? MeetingRoomName { get; set; }
    public string? MeetingDomain { get; set; }
    public string? MeetingUrl { get; set; }
    public string? MeetingRoomUrl { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }


    [ForeignKey(nameof(StudentId))]
    public virtual User Student { get; set; } = new User();

    [ForeignKey(nameof(TransactionId))]
    public virtual Transaction Transaction { get; set; } = new Transaction();

    [ForeignKey(nameof(ListingId))]
    public virtual Listing Listing { get; set; } = new Listing();

    public override string ToString()
    {
        return $"Lesson: {Id}, StudentId: {StudentId}, ListingId: {ListingId}, Date: {Date}, Duration: {Duration}, Price: {Price:C}, Status: {Status}";
    }
}

