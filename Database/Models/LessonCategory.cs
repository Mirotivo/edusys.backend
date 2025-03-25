using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class LessonCategory
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(255)]
    public string Name { get; set; }
    public bool DisplayInLandingPage { get; set; } = false;

    [Required]
    public string ImageUrl { get; set; }
    public virtual ICollection<ListingLessonCategory> ListingLessonCategories { get; set; };
}

