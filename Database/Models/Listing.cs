using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


public class Listing : IOwnableAccountable
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Hourly rate must be a positive value.")]
    public decimal HourlyRate { get; set; }

    public ListingLocationType Locations { get; set; }

    public bool IsVisible { get; set; } = true;

    public bool DisplayInLandingPage { get; set; } = false;

    public bool Active { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }


    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; } = new User();
    public virtual ICollection<ListingLessonCategory> ListingLessonCategories { get; set; } = new List<ListingLessonCategory>();

    public override string ToString()
    {
        return $"Listing: {Id}, Title: {Name}, UserId: {UserId}, IsVisible: {IsVisible}";
    }
}

