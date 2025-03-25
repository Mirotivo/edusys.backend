using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

public class ListingStatisticsDto
{
    public int TotalListings { get; set; }
    public int NewListingsToday { get; set; }
}

public class ListingDto
{
    public int Id { get; set; }
    public string TutorId { get; set; }
    public string TutorName { get; set; }
    public string TutorBio { get; set; }
    public int ContactedCount { get; set; }
    public AddressDto? TutorAddress { get; set; }
    public int Reviews { get; set; }
    public string LessonCategory { get; set; }
    public string Title { get; set; }
    public string? ListingImagePath { get; set; }
    public IFormFile? ListingImage { get; set; }
    public List<string> Locations { get; set; }
    public string AboutLesson { get; set; }
    public string AboutYou { get; set; }
    public string Rate { get; set; }
    public RatesDto Rates { get; set; }
    public List<string> SocialPlatforms { get; set; }
    public bool IsVisible { get; set; }

    public ListingDto()
    {
        TutorName = string.Empty;
        TutorId = string.Empty;
        TutorBio = string.Empty;
        LessonCategory = string.Empty;
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

public class UpdateTitleDto
{
    public string Title { get; set; } = string.Empty;
}

public class UpdateDescriptionDto
{
    public string AboutLesson { get; set; } = string.Empty;
    public string AboutYou { get; set; } = string.Empty;
}

public class UpdateCategoryDto
{
    public int LessonCategoryId { get; set; }
}

public class UpdateLocationsDto
{
    public List<string> Locations { get; set; } = new List<string>();
}

