using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


public class Listing : IOwnableAccountable
{
    [Key]
    public int Id { get; set; }

    public string UserId { get; set; }

    [ForeignKey(nameof(Listing.UserId))]
    public User? User { get; set; }

    [MaxLength(100)]
    public string Title { get; set; }

    [MaxLength(500)]
    public string Description { get; set; }

    [Required]
    public ListingRates Rates { get; set; }

    [MaxLength(255)]
    public string? ListingImagePath { get; set; }

    public ListingLocationType Locations { get; set; }

    [MaxLength(500)]
    public string AboutYou { get; set; }

    [MaxLength(500)]
    public string AboutLesson { get; set; }

    public bool IsVisible { get; set; } = true;

    public ICollection<ListingLessonCategory> ListingLessonCategories { get; set; } = new List<ListingLessonCategory>();

    public bool Active { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    public Listing()
    {
        UserId = string.Empty;
        Title = string.Empty;
        ListingImagePath = string.Empty;
        AboutYou = string.Empty;
        AboutLesson = string.Empty;
        Description = string.Empty;
        Rates = new ListingRates();
    }

    public override string ToString()
    {
        return $"Listing: {Id}, Title: {Title}, UserId: {UserId}, IsVisible: {IsVisible}";
    }
}

