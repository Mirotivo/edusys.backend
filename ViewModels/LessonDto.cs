public class LessonDto
{
    public int Id { get; set; }
    public string? Topic { get; set; } // Always "Lesson" in this context
    public DateTime Date { get; set; } // The date of the lesson
    public TimeSpan Duration { get; set; }
    public decimal Price { get; set; }
    public string? StudentId { get; set; }
    public int ListingId { get; set; }
    public LessonStatus? Status { get; set; } // Status of the lesson (e.g., "Booked", "Completed", "Canceled")
    public string? MeetingToken { get; set; }
    public string? MeetingDomain { get; set; }
    public string? MeetingUrl { get; set; }
    public string? MeetingRoomUrl { get; set; }
    public string? MeetingRoomName { get; set; }
    public string? StudentName { get; set; }
    public string? TutorName { get; set; }
    public string? RecipientName { get; set; }
}
