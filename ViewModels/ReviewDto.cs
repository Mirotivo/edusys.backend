public class ReviewDto
{
    public string RevieweeId { get; set; }
    public string? Name { get; set; }
    public string? Subject { get; set; }
    public string? Feedback { get; set; } // For received or sent reviews
    public string? Avatar { get; set; } // Optional avatar URL
    public int? Rating { get; set; }

    public ReviewDto()
    {
        RevieweeId = string.Empty;
    }
}
