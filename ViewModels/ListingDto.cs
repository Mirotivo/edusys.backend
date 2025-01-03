public class ListingDto
{
    public int Id { get; set; }
    public string TutorId { get; set; }
    public string Name { get; set; }
    public int ContactedCount { get; set; }
    public string Category { get; set; }
    public string Title { get; set; }
    public string? ListingImagePath { get; set; }
    public IFormFile? ListingImage { get; set; }
    public List<string> Locations { get; set; }
    public string AboutLesson { get; set; }
    public string AboutYou { get; set; }
    public string Rate { get; set; }
    public RatesDto Rates { get; set; }
    public List<string> SocialPlatforms { get; set; }

    public ListingDto()
    {
        Name = string.Empty;
        TutorId = string.Empty;
        Category = string.Empty;
        Title = string.Empty;
        AboutLesson = string.Empty;
        AboutYou = string.Empty;
        Rate = string.Empty;
        Locations = new List<string>();
        SocialPlatforms = new List<string>();
        Rates = new RatesDto();
    }
}

public class RatesDto
{
    public decimal Hourly { get; set; }
    public decimal FiveHours { get; set; }
    public decimal TenHours { get; set; }
}

public class CreateListingDto
{
    public IFormFile? ListingImage { get; set; }
    public string? ListingImagePath { get; set; }

    public string Title { get; set; }
    public string AboutLesson { get; set; }
    public string AboutYou { get; set; }
    public List<string> Locations { get; set; }
    public int? LessonCategoryId { get; set; }
    public string? LessonCategory { get; set; }
    public RatesDto Rates { get; set; }

    public CreateListingDto()
    {
        Title = string.Empty;
        AboutLesson = string.Empty;
        AboutYou = string.Empty;
        Locations = new List<string>();
        Rates = new RatesDto();
    }
}
