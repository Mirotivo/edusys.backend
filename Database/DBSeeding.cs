using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;


public class RoleSeeder
{
    public static void Seed(skillseekDbContext dbContext)
    {
        var roleNames = Enum.GetNames(typeof(Role));

        foreach (var roleName in roleNames)
        {
            if (!dbContext.Roles.Any(r => r.Name == roleName))
            {
                dbContext.Roles.Add(new IdentityRole
                {
                    Name = roleName,
                    NormalizedName = roleName.ToUpper()
                });
            }
        }

        dbContext.SaveChanges();
    }
}





public class UserSeeder
{
    public static void Seed(skillseekDbContext context)
    {
        if (!context.Users.Any())
        {
            var users = new List<User>
            {
                new User
                {
                    FirstName = "Amr",
                    LastName = "Mostafa",
                    Email = "Amr.Mostafa@live.com",
                    Address = "76 Bancroft Ave, Roseville NSW 2069, Australia",
                    ProfileImagePath = $"https://robohash.org/{Guid.NewGuid()}?size=200x200&set=set1",
                    // Password = "123456",
                    PasswordHash = "AQAAAAIAAYagAAAAELHjO0Ma6EpZO1UcrJ0FEJu4iXQk3jBFFn8c1p0m0r0UatkNq7uUj0B//Hn/gj/drQ==",
                },
                new User
                {
                    FirstName = "Abdelrahman",
                    LastName = "Tarek",
                    Email = "Abdelrahman.Tarek@live.com",
                    Address = "15 George St, Sydney NSW 2000, Australia",
                    ProfileImagePath = $"https://robohash.org/{Guid.NewGuid()}?size=200x200&set=set1",
                    // Password = "123456",
                    PasswordHash = "AQAAAAIAAYagAAAAELHjO0Ma6EpZO1UcrJ0FEJu4iXQk3jBFFn8c1p0m0r0UatkNq7uUj0B//Hn/gj/drQ==",
                },
                new User
                {
                    FirstName = "Amir",
                    LastName = "Salah",
                    Email = "Amir.Salah@live.com",
                    Address = "9 Smith St, Chatswood NSW 2067, Australia",
                    ProfileImagePath = $"https://robohash.org/{Guid.NewGuid()}?size=200x200&set=set1",
                    // Password = "123456",
                    PasswordHash = "AQAAAAIAAYagAAAAELHjO0Ma6EpZO1UcrJ0FEJu4iXQk3jBFFn8c1p0m0r0UatkNq7uUj0B//Hn/gj/drQ==",
                },
                new User
                {
                    FirstName = "Mostafa",
                    LastName = "Salah",
                    Email = "Mostafa.Salah@live.com",
                    Address = "3 Greenway Dr, Castle Hill NSW 2154, Australia",
                    ProfileImagePath = $"https://robohash.org/{Guid.NewGuid()}?size=200x200&set=set1",
                    // Password = "123456",
                    PasswordHash = "AQAAAAIAAYagAAAAELHjO0Ma6EpZO1UcrJ0FEJu4iXQk3jBFFn8c1p0m0r0UatkNq7uUj0B//Hn/gj/drQ==",
                },
                new User
                {
                    FirstName = "Martineo",
                    LastName = "",
                    Email = "Martineo",
                    Address = "47 Miller St, North Sydney NSW 2060, Australia",
                    ProfileImagePath = $"https://robohash.org/{Guid.NewGuid()}?size=200x200&set=set1",
                    // Password = "123456",
                    PasswordHash = "AQAAAAIAAYagAAAAELHjO0Ma6EpZO1UcrJ0FEJu4iXQk3jBFFn8c1p0m0r0UatkNq7uUj0B//Hn/gj/drQ==",
                },
                new User
                {
                    FirstName = "Yurika",
                    LastName = "",
                    Email = "Yurika",
                    Address = "21 Pacific Hwy, Hornsby NSW 2077, Australia",
                    ProfileImagePath = $"https://robohash.org/{Guid.NewGuid()}?size=200x200&set=set1",
                    // Password = "123456",
                    PasswordHash = "AQAAAAIAAYagAAAAELHjO0Ma6EpZO1UcrJ0FEJu4iXQk3jBFFn8c1p0m0r0UatkNq7uUj0B//Hn/gj/drQ==",
                },
                new User
                {
                    FirstName = "Timothy",
                    LastName = "",
                    Email = "Timothy",
                    Address = "8 Hunter St, Parramatta NSW 2150, Australia",
                    ProfileImagePath = $"https://robohash.org/{Guid.NewGuid()}?size=200x200&set=set1",
                    // Password = "123456",
                    PasswordHash = "AQAAAAIAAYagAAAAELHjO0Ma6EpZO1UcrJ0FEJu4iXQk3jBFFn8c1p0m0r0UatkNq7uUj0B//Hn/gj/drQ==",
                },
                new User
                {
                    FirstName = "Imke",
                    LastName = "",
                    Email = "Imke",
                    Address = "12 Macquarie St, Liverpool NSW 2170, Australia",
                    ProfileImagePath = $"https://robohash.org/{Guid.NewGuid()}?size=200x200&set=set1",
                    // Password = "123456",
                    PasswordHash = "AQAAAAIAAYagAAAAELHjO0Ma6EpZO1UcrJ0FEJu4iXQk3jBFFn8c1p0m0r0UatkNq7uUj0B//Hn/gj/drQ==",
                },
                new User
                {
                    FirstName = "Aziz",
                    LastName = "",
                    Email = "Aziz",
                    Address = "10 Park Ave, Blacktown NSW 2148, Australia",
                    ProfileImagePath = $"https://robohash.org/{Guid.NewGuid()}?size=200x200&set=set1",
                    // Password = "123456",
                    PasswordHash = "AQAAAAIAAYagAAAAELHjO0Ma6EpZO1UcrJ0FEJu4iXQk3jBFFn8c1p0m0r0UatkNq7uUj0B//Hn/gj/drQ==",
                },
                new User
                {
                    FirstName = "Alice",
                    LastName = "",
                    Email = "Alice",
                    Address = "5 Victoria Rd, Ryde NSW 2112, Australia",
                    ProfileImagePath = $"https://robohash.org/{Guid.NewGuid()}?size=200x200&set=set1",
                    // Password = "123456",
                    PasswordHash = "AQAAAAIAAYagAAAAELHjO0Ma6EpZO1UcrJ0FEJu4iXQk3jBFFn8c1p0m0r0UatkNq7uUj0B//Hn/gj/drQ==",
                },
                new User
                {
                    FirstName = "Eloise",
                    LastName = "",
                    Email = "Eloise",
                    Address = "7 Phillip St, Penrith NSW 2750, Australia",
                    ProfileImagePath = $"https://robohash.org/{Guid.NewGuid()}?size=200x200&set=set1",
                    // Password = "123456",
                    PasswordHash = "AQAAAAIAAYagAAAAELHjO0Ma6EpZO1UcrJ0FEJu4iXQk3jBFFn8c1p0m0r0UatkNq7uUj0B//Hn/gj/drQ==",
                },
                new User
                {
                    FirstName = "Seun",
                    LastName = "",
                    Email = "Seun",
                    Address = "23 Bridge St, Erskineville NSW 2043, Australia",
                    ProfileImagePath = $"https://robohash.org/{Guid.NewGuid()}?size=200x200&set=set1",
                    // Password = "123456",
                    PasswordHash = "AQAAAAIAAYagAAAAELHjO0Ma6EpZO1UcrJ0FEJu4iXQk3jBFFn8c1p0m0r0UatkNq7uUj0B//Hn/gj/drQ==",
                },
                new User
                {
                    FirstName = "Anthea",
                    LastName = "",
                    Email = "Anthea",
                    Address = "17 Railway Parade, Burwood NSW 2134, Australia",
                    ProfileImagePath = $"https://robohash.org/{Guid.NewGuid()}?size=200x200&set=set1",
                    // Password = "123456",
                    PasswordHash = "AQAAAAIAAYagAAAAELHjO0Ma6EpZO1UcrJ0FEJu4iXQk3jBFFn8c1p0m0r0UatkNq7uUj0B//Hn/gj/drQ==",
                },
                new User
                {
                    FirstName = "Tanishka",
                    LastName = "",
                    Email = "Tanishka",
                    Address = "42 Oxford St, Bondi Junction NSW 2022, Australia",
                    ProfileImagePath = $"https://robohash.org/{Guid.NewGuid()}?size=200x200&set=set1",
                    // Password = "123456",
                    PasswordHash = "AQAAAAIAAYagAAAAELHjO0Ma6EpZO1UcrJ0FEJu4iXQk3jBFFn8c1p0m0r0UatkNq7uUj0B//Hn/gj/drQ==",
                },
                new User
                {
                    FirstName = "Student",
                    LastName = "",
                    Email = "Student@gmail.com",
                    Address = "22 Bronte Road, Bondi Junction NSW 2022, Australia",
                    ProfileImagePath = $"https://robohash.org/{Guid.NewGuid()}?size=200x200&set=set1",
                    // Password = "123456",
                    PasswordHash = "AQAAAAIAAYagAAAAELHjO0Ma6EpZO1UcrJ0FEJu4iXQk3jBFFn8c1p0m0r0UatkNq7uUj0B//Hn/gj/drQ==",
                },
                new User
                {
                    FirstName = "Tutor",
                    LastName = "",
                    Email = "Tutor@gmail.com",
                    Address = "101 Grafton Street, Bondi Junction NSW 2022, Australia",
                    ProfileImagePath = $"https://robohash.org/{Guid.NewGuid()}?size=200x200&set=set1",
                    // Password = "123456",
                    PasswordHash = "AQAAAAIAAYagAAAAELHjO0Ma6EpZO1UcrJ0FEJu4iXQk3jBFFn8c1p0m0r0UatkNq7uUj0B//Hn/gj/drQ==",
                },
            };

            context.Users.AddRange(users);
            context.SaveChanges();
        }
    }
}








public class LessonCategorySeeder
{
    public static void Seed(skillseekDbContext context)
    {
        if (!context.LessonCategories.Any())
        {
            var categories = new List<LessonCategory>
            {
                new LessonCategory { Name = "Maths" },
                new LessonCategory { Name = "English" },
                new LessonCategory { Name = "Piano" },
                new LessonCategory { Name = "Singing" },
                new LessonCategory { Name = "Japanese" },
                new LessonCategory { Name = "Spanish" },
                new LessonCategory { Name = "French" },
                new LessonCategory { Name = "Swimming" },
                new LessonCategory { Name = "Guitar" },
                new LessonCategory { Name = "Electrical" },
                new LessonCategory { Name = "Physics" },
                new LessonCategory { Name = "Programming" },
                new LessonCategory { Name = "Chemistry" },
                new LessonCategory { Name = "Art" },
                new LessonCategory { Name = "Music" },
            };

            context.LessonCategories.AddRange(categories);
            context.SaveChanges();
        }
    }
}


public class ListingSeeder
{
    public static void Seed(skillseekDbContext context)
    {
        if (!context.Listings.Any())
        {
            var listings = new List<Listing>
            {
                new Listing
                {
                    UserId = context.Users.FirstOrDefault(c => EF.Functions.Like(c.Email, "Amr.Mostafa@live.com"))?.Id ?? string.Empty,
                    LessonCategoryId = context.LessonCategories.FirstOrDefault(c => EF.Functions.Like(c.Name, "Electrical"))?.Id ?? 0,
                    Rates = new ListingRates() { Hourly = 40.0M, FiveHours = 40.0M * 5, TenHours = 40.0M * 10 },
                    Title = "Academic tutoring",
                    Description = "Specialize in computer architecture and FPGA",
                    ListingImagePath = $"https://robohash.org/{Guid.NewGuid()}?size=200x200&set=set1",
                    Locations = LocationType.Webcam | LocationType.StudentLocation | LocationType.TutorLocation,
                    AboutYou = "My name is Amr, and I’m a computer engineer...",
                    AboutLesson = "Lessons are instructional sessions designed...",
                },
                new Listing
                {
                    UserId = context.Users.FirstOrDefault(c => EF.Functions.Like(c.Email, "Amr.Mostafa@live.com"))?.Id ?? string.Empty,
                    LessonCategoryId = context.LessonCategories.FirstOrDefault(c => EF.Functions.Like(c.Name, "Maths"))?.Id ?? 0,
                    Rates = new ListingRates() { Hourly = 40.0M, FiveHours = 40.0M * 5, TenHours = 40.0M * 10 },
                    Title = "Postgraduate Math tutoring",
                    Description = "Ph.D. student providing advanced courses in Math",
                    ListingImagePath = $"https://robohash.org/{Guid.NewGuid()}?size=200x200&set=set1",
                    Locations = LocationType.Webcam | LocationType.StudentLocation | LocationType.TutorLocation,
                    AboutYou = "My name is Amr, and I’m a computer engineer...",
                    AboutLesson = "Mathematics lessons tailored for students...",
                },
                new Listing
                {
                    UserId = context.Users.FirstOrDefault(c => EF.Functions.Like(c.Email, "Amr.Mostafa@live.com"))?.Id ?? string.Empty,
                    LessonCategoryId = context.LessonCategories.FirstOrDefault(c => EF.Functions.Like(c.Name, "English"))?.Id ?? 0,
                    Rates = new ListingRates() { Hourly = 50.0M, FiveHours = 50.0M * 5, TenHours = 50.0M * 10 },
                    Title = "English tutoring",
                    Description = "Experienced academic tutor helping improve writing and speaking skills",
                    ListingImagePath = $"https://robohash.org/{Guid.NewGuid()}?size=200x200&set=set1",
                    Locations = LocationType.Webcam,
                    AboutYou = "My name is Amr, and I specialize in academic tutoring...",
                    AboutLesson = "Focus on English speaking, writing, and grammar skills...",
                },
                new Listing
                {
                    UserId = context.Users.FirstOrDefault(c => EF.Functions.Like(c.Email, "Amr.Mostafa@live.com"))?.Id ?? string.Empty,
                    LessonCategoryId = context.LessonCategories.FirstOrDefault(c => EF.Functions.Like(c.Name, "Guitar"))?.Id ?? 0,
                    Rates = new ListingRates() { Hourly = 60.0M, FiveHours = 60.0M * 5, TenHours = 60.0M * 10 },
                    Title = "Guitar lessons",
                    Description = "Mount Lawley-based studio offering personalized guitar lessons",
                    ListingImagePath = $"https://robohash.org/{Guid.NewGuid()}?size=200x200&set=set1",
                    Locations = LocationType.Webcam | LocationType.TutorLocation,
                    AboutYou = "I have over 10 years of experience teaching guitar to students of all levels...",
                    AboutLesson = "Learn guitar basics, chords, and advanced techniques...",
                },
                // Add random listings with varying details
                new Listing
                {
                    UserId = context.Users.FirstOrDefault(c => EF.Functions.Like(c.Email, "Abdelrahman.Tarek@live.com"))?.Id ?? string.Empty,
                    LessonCategoryId = context.LessonCategories.FirstOrDefault(c => EF.Functions.Like(c.Name, "Physics"))?.Id ?? 0,
                    Rates = new ListingRates() { Hourly = 70.0M, FiveHours = 70.0M * 5, TenHours = 70.0M * 10 },
                    Title = "Physics tutoring",
                    Description = "Experienced tutor helping students excel in Physics concepts",
                    ListingImagePath = $"https://robohash.org/{Guid.NewGuid()}?size=200x200&set=set1",
                    Locations = LocationType.Webcam | LocationType.StudentLocation,
                    AboutYou = "Physics graduate with 5+ years of teaching experience...",
                    AboutLesson = "Interactive sessions to simplify complex Physics concepts...",
                },
                new Listing
                {
                    UserId = context.Users.FirstOrDefault(c => EF.Functions.Like(c.Email, "Alice"))?.Id ?? string.Empty,
                    LessonCategoryId = context.LessonCategories.FirstOrDefault(c => EF.Functions.Like(c.Name, "Programming"))?.Id ?? 0,
                    Rates = new ListingRates() { Hourly = 80.0M, FiveHours = 80.0M * 5, TenHours = 80.0M * 10 },
                    Title = "Programming for beginners",
                    Description = "Learn Python, Java, and web development from scratch",
                    ListingImagePath = $"https://robohash.org/{Guid.NewGuid()}?size=200x200&set=set1",
                    Locations = LocationType.Webcam,
                    AboutYou = "Professional software developer with 8 years of experience...",
                    AboutLesson = "Practical programming lessons for beginners and intermediate learners...",
                },
                new Listing
                {
                    UserId = context.Users.FirstOrDefault(c => EF.Functions.Like(c.Email, "Tanishka"))?.Id ?? string.Empty,
                    LessonCategoryId = context.LessonCategories.FirstOrDefault(c => EF.Functions.Like(c.Name, "Chemistry"))?.Id ?? 0,
                    Rates = new ListingRates() { Hourly = 55.0M, FiveHours = 55.0M * 5, TenHours = 55.0M * 10 },
                    Title = "Chemistry made simple",
                    Description = "Breaking down complex Chemistry topics for students",
                    ListingImagePath = $"https://robohash.org/{Guid.NewGuid()}?size=200x200&set=set1",
                    Locations = LocationType.Webcam | LocationType.StudentLocation,
                    AboutYou = "Chemistry teacher with a passion for making science fun...",
                    AboutLesson = "Comprehensive Chemistry lessons with real-world applications...",
                },
                new Listing
                {
                    UserId = context.Users.FirstOrDefault(c => EF.Functions.Like(c.Email, "Anthea"))?.Id ?? string.Empty,
                    LessonCategoryId = context.LessonCategories.FirstOrDefault(c => EF.Functions.Like(c.Name, "Art"))?.Id ?? 0,
                    Rates = new ListingRates() { Hourly = 45.0M, FiveHours = 45.0M * 5, TenHours = 45.0M * 10 },
                    Title = "Beginner Art Classes",
                    Description = "Learn the fundamentals of drawing and painting",
                    ListingImagePath = $"https://robohash.org/{Guid.NewGuid()}?size=200x200&set=set1",
                    Locations = LocationType.Webcam | LocationType.TutorLocation,
                    AboutYou = "Artist with 6 years of experience teaching beginners...",
                    AboutLesson = "Step-by-step lessons to master the basics of drawing and painting...",
                },
                new Listing
                {
                    UserId = context.Users.FirstOrDefault(c => EF.Functions.Like(c.Email, "Seun"))?.Id ?? string.Empty,
                    LessonCategoryId = context.LessonCategories.FirstOrDefault(c => EF.Functions.Like(c.Name, "Music"))?.Id ?? 0,
                    Rates = new ListingRates() { Hourly = 75.0M, FiveHours = 75.0M * 5, TenHours = 75.0M * 10 },
                    Title = "Piano Lessons",
                    Description = "Personalized piano lessons for all levels",
                    ListingImagePath = $"https://robohash.org/{Guid.NewGuid()}?size=200x200&set=set1",
                    Locations = LocationType.Webcam | LocationType.StudentLocation,
                    AboutYou = "Professional pianist offering lessons to inspire creativity...",
                    AboutLesson = "Learn piano techniques, reading sheet music, and more...",
                },
                new Listing
                {
                    UserId = context.Users.FirstOrDefault(c => EF.Functions.Like(c.Email, "Tutor@gmail.com"))?.Id ?? string.Empty,
                    LessonCategoryId = context.LessonCategories.FirstOrDefault(c => EF.Functions.Like(c.Name, "Music"))?.Id ?? 0,
                    Rates = new ListingRates() { Hourly = 75.0M, FiveHours = 75.0M * 5, TenHours = 75.0M * 10 },
                    Title = "Piano Lessons",
                    Description = "Personalized piano lessons for all levels",
                    ListingImagePath = $"https://robohash.org/{Guid.NewGuid()}?size=200x200&set=set1",
                    Locations = LocationType.Webcam | LocationType.StudentLocation,
                    AboutYou = "Professional pianist offering lessons to inspire creativity...",
                    AboutLesson = "Learn piano techniques, reading sheet music, and more...",
                }
            };

            context.Listings.AddRange(listings);
            context.SaveChanges();
        }
    }
}


public class LessonSeeder
{
    public static void Seed(skillseekDbContext context)
    {
        if (!context.Lessons.Any())
        {
            var lessons = new List<Lesson>
            {
                // new Lesson
                // {
                //     Date = new DateTime(2024, 12, 1, 14, 0, 0),
                //     Duration = TimeSpan.FromHours(1),
                //     Price = 30.00m,
                //     StudentId = context.Users.FirstOrDefault(c => EF.Functions.Like(c.Email, "Abdelrahman.Tarek@live.com"))?.Id ?? 0,
                //     ListingId = context.Listings.FirstOrDefault(l => EF.Functions.Like(l.User.Email, "Amr.Mostafa@live.com"))?.Id ?? 0,
                //     IsStudentInitiated = true,
                //     Status = LessonStatus.Proposed
                // },
                // new Lesson
                // {
                //     Date = new DateTime(2024, 12, 1, 14, 0, 0),
                //     Duration = TimeSpan.FromHours(1),
                //     Price = 30.00m,
                //     StudentId = context.Users.FirstOrDefault(c => EF.Functions.Like(c.Email, "Abdelrahman.Tarek@live.com"))?.Id ?? 0,
                //     ListingId = context.Listings.FirstOrDefault(l => EF.Functions.Like(l.User.Email, "Amr.Mostafa@live.com"))?.Id ?? 0,
                //     IsStudentInitiated = false,
                //     Status = LessonStatus.Proposed
                // },
                // new Lesson
                // {
                //     Date = new DateTime(2024, 12, 3, 10, 0, 0),
                //     Duration = TimeSpan.FromHours(2),
                //     Price = 60.00m,
                //     StudentId = context.Users.FirstOrDefault(c => EF.Functions.Like(c.Email, "Abdelrahman.Tarek@live.com"))?.Id ?? 0,
                //     ListingId = context.Listings.FirstOrDefault(l => EF.Functions.Like(l.User.Email, "Amr.Mostafa@live.com"))?.Id ?? 0,
                //     IsStudentInitiated = false,
                //     Status = LessonStatus.Booked
                // },
                // new Lesson
                // {
                //     Date = new DateTime(2024, 12, 5, 18, 0, 0),
                //     Duration = TimeSpan.FromMinutes(90),
                //     Price = 45.00m,
                //     StudentId = context.Users.FirstOrDefault(c => EF.Functions.Like(c.Email, "Abdelrahman.Tarek@live.com"))?.Id ?? 0,
                //     ListingId = context.Listings.Where(l => EF.Functions.Like(l.User.Email, "Amr.Mostafa@live.com")).OrderBy(l => l.Id).Skip(1).Select(l => l.Id).FirstOrDefault(),
                //     IsStudentInitiated = true,
                //     Status = LessonStatus.Completed
                // },
                // new Lesson
                // {
                //     Date = new DateTime(2024, 12, 7, 16, 30, 0),
                //     Duration = TimeSpan.FromHours(2),
                //     Price = 50.00m,
                //     StudentId = context.Users.FirstOrDefault(c => EF.Functions.Like(c.Email, "Abdelrahman.Tarek@live.com"))?.Id ?? 0,
                //     ListingId = context.Listings.Where(l => EF.Functions.Like(l.User.Email, "Amr.Mostafa@live.com")).OrderBy(l => l.Id).Skip(1).Select(l => l.Id).FirstOrDefault(),
                //     IsStudentInitiated = false,
                //     Status = LessonStatus.Canceled
                // }
            };

            context.Lessons.AddRange(lessons);
            context.SaveChanges();
        }
    }
}


public class ReviewSeeder
{
    public static void Seed(skillseekDbContext context)
    {
        if (!context.Reviews.Any())
        {
            var reviews = new List<Review>
            {
                new Review
                {
                    ReviewerId = context.Users.FirstOrDefault(c => EF.Functions.Like(c.Email, "Abdelrahman.Tarek@live.com"))?.Id ?? string.Empty,
                    RevieweeId = context.Users.FirstOrDefault(c => EF.Functions.Like(c.Email, "Amr.Mostafa@live.com"))?.Id ?? string.Empty,
                    Rating = 5,
                    Title = "C++ student",
                    Comments = "Great tutor! Very helpful and knowledgeable."
                },
                new Review
                {
                    ReviewerId = context.Users.FirstOrDefault(c => EF.Functions.Like(c.Email, "Martineo"))?.Id ?? string.Empty,
                    RevieweeId = context.Users.FirstOrDefault(c => EF.Functions.Like(c.Email, "Amr.Mostafa@live.com"))?.Id ?? string.Empty,
                    Rating = 5,
                    Title = "C++ student",
                    Comments = "Amr is really patient and professional. Highly recommended!."
                },
                new Review
                {
                    ReviewerId = context.Users.FirstOrDefault(c => EF.Functions.Like(c.Email, "Yurika"))?.Id ?? string.Empty,
                    RevieweeId = context.Users.FirstOrDefault(c => EF.Functions.Like(c.Email, "Amr.Mostafa@live.com"))?.Id ?? string.Empty,
                    Rating = 5,
                    Title = "Python student",
                    Comments = "He tried his best to help me out. A good guy."
                },
                new Review
                {
                    ReviewerId = context.Users.FirstOrDefault(c => EF.Functions.Like(c.Email, "Timothy"))?.Id ?? string.Empty,
                    RevieweeId = context.Users.FirstOrDefault(c => EF.Functions.Like(c.Email, "Amr.Mostafa@live.com"))?.Id ?? string.Empty,
                    Rating = 5,
                    Title = "C++ student",
                    Comments = "Very friendly and knowledgeable."
                },
                new Review
                {
                    ReviewerId = context.Users.FirstOrDefault(c => EF.Functions.Like(c.Email, "Amr.Mostafa@live.com"))?.Id ?? string.Empty,
                    RevieweeId = context.Users.FirstOrDefault(c => EF.Functions.Like(c.Email, "Abdelrahman.Tarek@live.com"))?.Id ?? string.Empty,
                    Rating = 4,
                    Title = "C++ student",
                    Comments = "The student was attentive but needed more preparation for the lesson."
                },
            };

            context.Reviews.AddRange(reviews);
            context.SaveChanges();
        }
    }
}



public class ChatSeeder
{
    public static void Seed(skillseekDbContext context)
    {
        if (!context.Chats.Any())
        {
            var chats = new List<Chat>
            {
                new Chat
                {
                    StudentId = context.Users.FirstOrDefault(c => EF.Functions.Like(c.Email, "Abdelrahman.Tarek@live.com"))?.Id ?? string.Empty,
                    TutorId = context.Users.FirstOrDefault(c => EF.Functions.Like(c.Email, "Amr.Mostafa@live.com"))?.Id ?? string.Empty,
                    ListingId = context.Listings.FirstOrDefault(l => l.User != null && EF.Functions.Like(l.User.Email, "Amr.Mostafa@live.com"))?.Id ?? 0,
                },
                new Chat
                {
                    StudentId = context.Users.FirstOrDefault(c => EF.Functions.Like(c.Email, "Student@gmail.com"))?.Id ?? string.Empty,
                    TutorId = context.Users.FirstOrDefault(c => EF.Functions.Like(c.Email, "Tutor@gmail.com"))?.Id ?? string.Empty,
                    ListingId = context.Listings.FirstOrDefault(l => l.User != null && EF.Functions.Like(l.User.Email, "Tutor@gmail.com"))?.Id ?? 0,
                },
            };

            context.Chats.AddRange(chats);
            context.SaveChanges();
        }
    }
}



public class MessageSeeder
{
    public static void Seed(skillseekDbContext context)
    {
        if (!context.Messages.Any())
        {
            var chats = new List<Message>
            {
                new Message
                {
                    ChatId = 1,
                    SenderId = context.Users.FirstOrDefault(c => EF.Functions.Like(c.Email, "Abdelrahman.Tarek@live.com"))?.Id ?? string.Empty,
                    RecipientId = context.Users.FirstOrDefault(c => EF.Functions.Like(c.Email, "Amr.Mostafa@live.com"))?.Id ?? string.Empty,
                    Content = "Hi, I have a question about the lesson."
                },
                new Message
                {
                    ChatId = 1,
                    SenderId = context.Users.FirstOrDefault(c => EF.Functions.Like(c.Email, "Amr.Mostafa@live.com"))?.Id ?? string.Empty,
                    RecipientId = context.Users.FirstOrDefault(c => EF.Functions.Like(c.Email, "Abdelrahman.Tarek@live.com"))?.Id ?? string.Empty,
                    Content = "Sure!."
                },
                new Message
                {
                    ChatId = 1,
                    SenderId = context.Users.FirstOrDefault(c => EF.Functions.Like(c.Email, "Abdelrahman.Tarek@live.com"))?.Id ?? string.Empty,
                    RecipientId = context.Users.FirstOrDefault(c => EF.Functions.Like(c.Email, "Amr.Mostafa@live.com"))?.Id ?? string.Empty,
                    Content = "How did create this system?"
                },
                // new Message
                // {
                //     ChatId = 2,
                //     SenderId = context.Users.FirstOrDefault(c => EF.Functions.Like(c.Email, "Student@gmail.com"))?.Id ?? 0,
                //     RecipientId = context.Users.FirstOrDefault(c => EF.Functions.Like(c.Email, "Tutor@gmail.com"))?.Id ?? 0,
                //     Content = "Can you teach me?"
                // },
                // new Message
                // {
                //     ChatId = 2,
                //     SenderId = context.Users.FirstOrDefault(c => EF.Functions.Like(c.Email, "Tutor@gmail.com"))?.Id ?? 0,
                //     RecipientId = context.Users.FirstOrDefault(c => EF.Functions.Like(c.Email, "Student@gmail.com"))?.Id ?? 0,
                //     Content = "What lesson?"
                // }
            };

            context.Messages.AddRange(chats);
            context.SaveChanges();
        }
    }
}


