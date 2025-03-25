using System.ComponentModel.DataAnnotations;

public class LessonCategory
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(255)]
    public string Name { get; set; }
    public bool ShowInDashboard { get; set; } = false;
    public string? ImagePath { get; set; }

    public LessonCategory()
    {
        Name = string.Empty;
    }
}

