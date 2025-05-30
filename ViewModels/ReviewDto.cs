using System;

public class ReviewDto
{
    public string RevieweeId { get; set; }
    public DateTime? Date { get; set; }
    public string? Name { get; set; }
    public string? Subject { get; set; }
    public string? Feedback { get; set; }
    public string? Avatar { get; set; }
    public int? Rating { get; set; }

    public ReviewDto()
    {
        RevieweeId = string.Empty;
    }
}

