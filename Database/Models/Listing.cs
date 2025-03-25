using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


public class Listing : IOwnableAccountable
{
    [Key]
    public int Id { get; set; }
    public string UserId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Title { get; set; } // Name 

    [Required]
    [MaxLength(500)]
    public string Description { get; set; }

    [Required]
    public decimal HourRate { get; set; }

    public ListingLocationType Locations { get; set; } // webcam 


    // review 
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public bool Active { get; set; } 
    public bool IsVisible { get; set; } = true;
    public bool DisplayInLandingPage { get; set; } = false;


    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; }
    public virtual ICollection<ListingLessonCategory> ListingLessonCategories { get; set; } = new List<ListingLessonCategory>();

    public override string ToString()
    {
        return $"Listing: {Id}, Title: {Title}, UserId: {UserId}, IsVisible: {IsVisible}";
    }
}

