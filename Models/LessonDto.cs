public class LessonDto
{
    public int Id { get; set; }
    public string? Topic { get; set; } // Always "Lesson" in this context
    public DateTime Date { get; set; } // The date of the lesson
    public TimeSpan Duration { get; set; }
    public decimal Price { get; set; }
    public int? StudentId { get; set; }
    public int ListingId { get; set; }
    public LessonStatus? Status { get; set; } // Status of the lesson (e.g., "Booked", "Completed", "Canceled")
}