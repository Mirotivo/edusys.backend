public class ReviewDto
{
    public int RevieweeId { get; set; }
    public string? Name { get; set; }
    public string? Subject { get; set; }
    public string? Feedback { get; set; } // For received or sent reviews
    public string? Avatar { get; set; } // Optional avatar URL
}
