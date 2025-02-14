using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace avancira.Controllers;

[Route("api/landing")]
[ApiController]
public class LandingAPIController : BaseController
{
    private readonly IListingService _listingService;
    private readonly ILessonCategoryService _categoryService;
    private readonly ILogger<LandingAPIController> _logger;

    public LandingAPIController(
        IListingService listingService,
        ILessonCategoryService categoryService,
        ILogger<LandingAPIController> logger
    )
    {
        _listingService = listingService;
        _categoryService = categoryService;
        _logger = logger;
    }

    [HttpGet("categories")]
    public IActionResult Categories()
    {
        var categories = _categoryService.GetLandingPageCategories();
        return JsonOk(categories);
    }

    [HttpGet("courses")]
    public IActionResult GetCourses()
    {
        var listings = _listingService.GetLandingPageListings()
            .Select(course => new
            {
                listingId = course.Id,
                img = course.ListingImagePath ?? "default-img.jpg",
                lessonCategory = course.LessonCategory,
                title = course.Title,
                description = course.AboutLesson,
                rating = Math.Round(new Random().NextDouble() * (5.0 - 3.5) + 3.5, 1), // Random rating between 3.5 and 5
                reviews = new Random().Next(1000, 5000), // Random reviews between 1000 and 5000
                students = new Random().Next(500, 2000), // Random students between 500 and 2000
                price = $"${course.Rate}",
                instructor = course.TutorName,
                instructorImg = course.ListingImagePath ?? "default-instructor.jpg"
            }).ToList();

        return JsonOk(listings);
    }

    [HttpGet("trending-courses")]
    public IActionResult GetTrendingCourses()
    {
        var listings = _listingService.GetLandingPageListings()
            .Select(course => new
            {
                listingId = course.Id,
                img = course.ListingImagePath ?? "default-img.jpg",
                lessonCategory = course.LessonCategory,
                title = course.Title,
                description = course.AboutLesson,
                rating = Math.Round(new Random().NextDouble() * (5.0 - 3.5) + 3.5, 1), // Random rating between 3.5 and 5
                reviews = new Random().Next(1000, 5000), // Random reviews between 1000 and 5000
                students = new Random().Next(500, 2000), // Random students between 500 and 2000
                price = $"${course.Rate}",
                instructor = course.TutorName,
                instructorImg = course.ListingImagePath ?? "default-instructor.jpg"
            }).ToList();

        return JsonOk(listings);
    }


    [HttpGet("instructors")]
    public IActionResult GetInstructors()
    {
        var instructors = new List<object>
        {
            new { Img = "assets/img/user/user16.png", Name = "David Lee", Designation = "Web Developer", Rating = 5.0, Reviews = 2566, Students = 800 },
            new { Img = "assets/img/user/user17.png", Name = "Charlotte", Designation = "Designer", Rating = 4.8, Reviews = 2550, Students = 700 },
            new { Img = "assets/img/user/user18.png", Name = "Ethan Williams", Designation = "Marketing", Rating = 4.5, Reviews = 2500, Students = 850 }
        };

        return JsonOk(instructors);
    }

    [HttpGet("job-locations")]
    public IActionResult GetJobLocations()
    {
        var jobLocations = new List<object>
        {
            new { Img = "assets/img/city/city-07.jpg", City = "Paris", Country = "France", Mentors = 14 },
            new { Img = "assets/img/city/city-08.jpg", City = "Elpo", Country = "Mexico", Mentors = 18 },
            new { Img = "assets/img/city/city-09.jpg", City = "Buenos Aires", Country = "Argentina", Mentors = 22 }
        };

        return JsonOk(jobLocations);
    }

    [HttpGet("student-reviews")]
    public IActionResult GetStudentReviews()
    {
        var studentReviews = new List<object>
        {
            new { Img = "assets/img/user/user20.png", Name = "Hannah Schmitt", Position = "Lead Designer", Comment = "Great experience, highly recommend!", Rating = 5 },
            new { Img = "assets/img/user/user21.png", Name = "Anderson Saviour", Position = "IT Manager", Comment = "Very insightful lessons and great support.", Rating = 4 }
        };

        return JsonOk(studentReviews);
    }
}
