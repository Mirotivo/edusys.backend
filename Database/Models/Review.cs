using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


public class Review : IAccountable
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string ReviewerId { get; set; }

    [ForeignKey(nameof(Review.ReviewerId))]
    public User? Reviewer { get; set; }

    [Required]
    public string RevieweeId { get; set; }

    [ForeignKey(nameof(Review.RevieweeId))]
    public User? Reviewee { get; set; }

    [Required]
    public ReviewType Type { get; set; }

    [Range(1, 5)]
    public int? Rating { get; set; }

    [MaxLength(20)]
    public string Title { get; set; }

    [MaxLength(500)]
    public string Comments { get; set; }

    public bool Active { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    public Review()
    {
        ReviewerId = string.Empty;
        RevieweeId = string.Empty;
        Title = string.Empty;
        Comments = string.Empty;
    }

    public override string ToString()
    {
        return $"Review: {Id}, ReviewerId: {ReviewerId}, RevieweeId: {RevieweeId}, Rating: {Rating}, Type: {Type}, Title: {Title}";
    }
}

