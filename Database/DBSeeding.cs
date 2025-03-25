using System;
using System.Collections.Generic;
using System.Linq;
using Backend.Database.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;


public class PromoCodeSeeder
{
    public static void Seed(AvanciraDbContext context)
    {
        if (!context.PromoCodes.Any())
        {
            var promoCodes = new List<PromoCode>
            {
                new PromoCode
                {
                    Code = "WELCOMEFEB10",
                    DiscountAmount = 10m,
                    DiscountPercentage = 0,
                    ExpiryDate = DateTime.UtcNow.AddMonths(1),
                    IsActive = true
                },
                new PromoCode
                {
                    Code = "WELCOMEFEB20",
                    DiscountAmount = 20m,
                    DiscountPercentage = 0,
                    ExpiryDate = DateTime.UtcNow.AddMonths(1),
                    IsActive = true
                },
                new PromoCode
                {
                    Code = "WELCOMEFEB25PER",
                    DiscountAmount = 0,
                    DiscountPercentage = 25,
                    ExpiryDate = DateTime.UtcNow.AddMonths(1),
                    IsActive = true
                },
                new PromoCode
                {
                    Code = "WELCOMEFEB50PER",
                    DiscountAmount = 0,
                    DiscountPercentage = 50,
                    ExpiryDate = DateTime.UtcNow.AddMonths(1),
                    IsActive = true
                },
                new PromoCode
                {
                    Code = "WELCOMEFEBFREE",
                    DiscountAmount = 0,
                    DiscountPercentage = 100,
                    ExpiryDate = DateTime.UtcNow.AddMonths(1),
                    IsActive = true
                }
            };

            context.PromoCodes.AddRange(promoCodes);
            context.SaveChanges();
        }
    }
}



public class CountrySeeder
{
    public static void Seed(AvanciraDbContext context, UserManager<User> userManager)
    {
        if (!context.Countries.Any())
        {
            var countries = new List<Country>
            {
                new Country
                {
                    Name = "Australia",
                    Code = "AU"
                },
            };

            context.Countries.AddRange(countries);
            context.SaveChanges();
        }
    }
}



public class RoleSeeder
{
    public static void Seed(AvanciraDbContext context, UserManager<User> userManager)
    {
        var roleNames = Enum.GetNames(typeof(UserRole));

        foreach (var roleName in roleNames)
        {
            if (!context.Roles.Any(r => r.Name == roleName))
            {
                context.Roles.Add(new IdentityRole
                {
                    Name = roleName,
                    NormalizedName = roleName.ToUpper()
                });
            }
        }

        context.SaveChanges();
    }
}





public class UserSeeder
{
    public static void Seed(AvanciraDbContext context, UserManager<User> userManager)
    {
        if (!context.Users.Any())
        {
            var users = new List<User>
            {
                new User
                {
                    FirstName = "Tutor",
                    LastName = "",
                    Bio = "An experienced tutor in various subjects, passionate about helping students achieve their full potential. Offers personalized lessons in multiple areas to cater to diverse learning needs.",
                    UserName = "tutor@avancira.com",
                    Email = "tutor@avancira.com",
                    Address = new Address
                    {
                        StreetAddress = "101 Grafton Street",
                        City = "Bondi Junction",
                        State = "NSW",
                        Country = "Australia",
                        PostalCode = "2022",
                        Latitude = -33.8912,
                        Longitude = 151.2646,
                        FormattedAddress = "101 Grafton Street, Bondi Junction NSW 2022, Australia"
                    },
                    CountryId = context.Countries.FirstOrDefault(c => EF.Functions.Like(c.Code, "AU"))?.Id ?? null,
                    TimeZoneId = "Australia/Sydney",
                    ProfileImageUrl = $"https://robohash.org/{Guid.NewGuid()}?size=200x200&set=set1",
                },
                new User
                {
                    FirstName = "Student",
                    LastName = "",
                    Bio = "A dedicated student, always eager to learn and expand knowledge. Focused on achieving academic success with the support of talented tutors and mentors.",
                    UserName = "student@avancira.com",
                    Email = "student@avancira.com",
                    Address = new Address
                    {
                        StreetAddress = "22 Bronte Road",
                        City = "Bondi Junction",
                        State = "NSW",
                        Country = "Australia",
                        PostalCode = "2022",
                        Latitude = -33.8915,
                        Longitude = 151.2691,
                        FormattedAddress = "22 Bronte Road, Bondi Junction NSW 2022, Australia"
                    },
                    CountryId = context.Countries.FirstOrDefault(c => EF.Functions.Like(c.Code, "AU"))?.Id ?? null,
                    TimeZoneId = "Australia/Sydney",
                    ProfileImageUrl = $"https://robohash.org/{Guid.NewGuid()}?size=200x200&set=set1",
                },
                new User
                {
                    FirstName = "Amr",
                    LastName = "Mostafa",
                    Bio = "A software engineer with a passion for technology, AI, and problem-solving. Constantly improving skills and exploring new advancements in the tech world.",
                    UserName = "Amr.Mostafa@live.com",
                    Email = "Amr.Mostafa@live.com",
                    Address = new Address
                    {
                        StreetAddress = "76 Bancroft Ave",
                        City = "Roseville",
                        State = "NSW",
                        Country = "Australia",
                        PostalCode = "2069",
                        Latitude = -33.7812,
                        Longitude = 151.1731,
                        FormattedAddress = "76 Bancroft Ave, Roseville NSW 2069, Australia"
                    },
                    CountryId = context.Countries.FirstOrDefault(c => EF.Functions.Like(c.Code, "AU"))?.Id ?? null,
                    TimeZoneId = "Australia/Sydney",
                    ProfileImageUrl = $"assets/img/mentor/amr_mostafa.jpg",
                },
                new User
                {
                    FirstName = "Amir",
                    LastName = "Salah",
                    Bio = "A business strategist with experience in finance, marketing, and management. Passionate about helping companies grow through data-driven decisions.",
                    UserName = "Amir.Salah@live.com",
                    Email = "Amir.Salah@live.com",
                    Address = new Address
                    {
                        StreetAddress = "10 Market St",
                        City = "Sydney",
                        State = "NSW",
                        Country = "Australia",
                        PostalCode = "2000",
                        Latitude = -33.8696,
                        Longitude = 151.2069,
                        FormattedAddress = "10 Market St, Sydney NSW 2000, Australia"
                    },
                    CountryId = context.Countries.FirstOrDefault(c => EF.Functions.Like(c.Code, "AU"))?.Id ?? null,
                    TimeZoneId = "Australia/Sydney",
                    ProfileImageUrl = $"assets/img/mentor/amir_salah.jpg",
                },

                new User
                {
                    FirstName = "Ahmed",
                    LastName = "Mostafa",
                    Bio = "A creative professional specializing in photography, graphic design, and video editing. Passionate about storytelling through visuals.",
                    UserName = "Ahmed.Mostafa@live.com",
                    Email = "Ahmed.Mostafa@live.com",
                    Address = new Address
                    {
                        StreetAddress = "22 King St",
                        City = "Melbourne",
                        State = "VIC",
                        Country = "Australia",
                        PostalCode = "3000",
                        Latitude = -37.8170,
                        Longitude = 144.9622,
                        FormattedAddress = "22 King St, Melbourne VIC 3000, Australia"
                    },
                    CountryId = context.Countries.FirstOrDefault(c => EF.Functions.Like(c.Code, "AU"))?.Id ?? null,
                    TimeZoneId = "Australia/Melbourne",
                    ProfileImageUrl = $"assets/img/mentor/ahmed_mostafa.jpg",
                },
                new User
                {
                    FirstName = "Olivia",
                    LastName = "Brown",
                    Bio = "A talented musician and teacher with a deep passion for classical and contemporary music.",
                    UserName = "olivia.brown@avancira.com",
                    Email = "olivia.brown@avancira.com",
                    Address = new Address
                    {
                        StreetAddress = "15 York St",
                        City = "Perth",
                        State = "WA",
                        Country = "Australia",
                        PostalCode = "6000",
                        Latitude = -31.9535,
                        Longitude = 115.8575,
                        FormattedAddress = "15 York St, Perth WA 6000, Australia"
                    },
                    CountryId = context.Countries.FirstOrDefault(c => EF.Functions.Like(c.Code, "AU"))?.Id ?? null,
                    TimeZoneId = "Australia/Perth",
                    ProfileImageUrl = "assets/img/mentor/olivia_brown.jpg"
                },
                new User
                {
                    FirstName = "Ethan",
                    LastName = "Clark",
                    Bio = "An AI researcher focusing on deep learning and computer vision applications.",
                    UserName = "ethan.clark@avancira.com",
                    Email = "ethan.clark@avancira.com",
                    Address = new Address
                    {
                        StreetAddress = "32 Murray St",
                        City = "Brisbane",
                        State = "QLD",
                        Country = "Australia",
                        PostalCode = "4000",
                        Latitude = -27.4723,
                        Longitude = 153.0287,
                        FormattedAddress = "32 Murray St, Brisbane QLD 4000, Australia"
                    },
                    CountryId = context.Countries.FirstOrDefault(c => EF.Functions.Like(c.Code, "AU"))?.Id ?? null,
                    TimeZoneId = "Australia/Brisbane",
                    ProfileImageUrl = "assets/img/mentor/ethan_clark.jpg"
                },
                new User
                {
                    FirstName = "Sophia",
                    LastName = "Williams",
                    Bio = "A passionate teacher with experience in early childhood education and curriculum development.",
                    UserName = "sophia.williams@avancira.com",
                    Email = "sophia.williams@avancira.com",
                    Address = new Address
                    {
                        StreetAddress = "20 King St",
                        City = "Sydney",
                        State = "NSW",
                        Country = "Australia",
                        PostalCode = "2000",
                        Latitude = -33.8688,
                        Longitude = 151.2093,
                        FormattedAddress = "20 King St, Sydney NSW 2000, Australia"
                    },
                    CountryId = context.Countries.FirstOrDefault(c => EF.Functions.Like(c.Code, "AU"))?.Id ?? null,
                    TimeZoneId = "Australia/Sydney",
                    ProfileImageUrl = "assets/img/mentor/sophia_williams.jpg"
                },
                new User
                {
                    FirstName = "Liam",
                    LastName = "Johnson",
                    Bio = "A software engineer specializing in mobile app development and UX design.",
                    UserName = "liam.johnson@avancira.com",
                    Email = "liam.johnson@avancira.com",
                    Address = new Address
                    {
                        StreetAddress = "45 Collins St",
                        City = "Sydney",
                        State = "NSW",
                        Country = "Australia",
                        PostalCode = "2000",
                        Latitude = -33.8688,
                        Longitude = 151.2093,
                        FormattedAddress = "45 Collins St, Sydney NSW 2000, Australia"
                    },
                    CountryId = context.Countries.FirstOrDefault(c => EF.Functions.Like(c.Code, "AU"))?.Id ?? null,
                    TimeZoneId = "Australia/Sydney",
                    ProfileImageUrl = "assets/img/mentor/liam_johnson.jpg"
                },
                new User
                {
                    FirstName = "Ava",
                    LastName = "Smith",
                    Bio = "An experienced business analyst with expertise in market research and strategy.",
                    UserName = "ava.smith@avancira.com",
                    Email = "ava.smith@avancira.com",
                    Address = new Address
                    {
                        StreetAddress = "78 Victoria St",
                        City = "Brisbane",
                        State = "QLD",
                        Country = "Australia",
                        PostalCode = "4000",
                        Latitude = -27.4723,
                        Longitude = 153.0287,
                        FormattedAddress = "78 Victoria St, Brisbane QLD 4000, Australia"
                    },
                    CountryId = context.Countries.FirstOrDefault(c => EF.Functions.Like(c.Code, "AU"))?.Id ?? null,
                    TimeZoneId = "Australia/Brisbane",
                    ProfileImageUrl = "assets/img/mentor/ava_smith.jpg"
                },
                new User
                {
                    FirstName = "Noah",
                    LastName = "Davis",
                    Bio = "A physics professor with a passion for space exploration and astrophysics.",
                    UserName = "noah.davis@avancira.com",
                    Email = "noah.davis@avancira.com",
                    Address = new Address
                    {
                        StreetAddress = "12 Swan St",
                        City = "Perth",
                        State = "WA",
                        Country = "Australia",
                        PostalCode = "6000",
                        Latitude = -31.9535,
                        Longitude = 115.8575,
                        FormattedAddress = "12 Swan St, Perth WA 6000, Australia"
                    },
                    CountryId = context.Countries.FirstOrDefault(c => EF.Functions.Like(c.Code, "AU"))?.Id ?? null,
                    TimeZoneId = "Australia/Perth",
                    ProfileImageUrl = "assets/img/mentor/noah_davis.jpg"
                },
                new User
                {
                    FirstName = "Isabella",
                    LastName = "Martinez",
                    Bio = "A data scientist specializing in AI-driven financial analytics and forecasting.",
                    UserName = "isabella.martinez@avancira.com",
                    Email = "isabella.martinez@avancira.com",
                    Address = new Address
                    {
                        StreetAddress = "66 George St",
                        City = "Sydney",
                        State = "NSW",
                        Country = "Australia",
                        PostalCode = "2000",
                        Latitude = -33.8688,
                        Longitude = 151.2093,
                        FormattedAddress = "66 George St, Sydney NSW 2000, Australia"
                    },
                    CountryId = context.Countries.FirstOrDefault(c => EF.Functions.Like(c.Code, "AU"))?.Id ?? null,
                    TimeZoneId = "Australia/Sydney",
                    ProfileImageUrl = "assets/img/mentor/isabella_martinez.jpg"
                },
                new User
                {
                    FirstName = "James",
                    LastName = "Wilson",
                    Bio = "An expert in cybersecurity and ethical hacking.",
                    UserName = "james.wilson@avancira.com",
                    Email = "james.wilson@avancira.com",
                    Address = new Address
                    {
                        StreetAddress = "29 Edward St",
                        City = "Brisbane",
                        State = "QLD",
                        Country = "Australia",
                        PostalCode = "4000",
                        Latitude = -27.4723,
                        Longitude = 153.0287,
                        FormattedAddress = "29 Edward St, Brisbane QLD 4000, Australia"
                    },
                    CountryId = context.Countries.FirstOrDefault(c => EF.Functions.Like(c.Code, "AU"))?.Id ?? null,
                    TimeZoneId = "Australia/Brisbane",
                    ProfileImageUrl = "assets/img/mentor/james_wilson.jpg"
                },
                new User
                {
                    FirstName = "Charlotte",
                    LastName = "Anderson",
                    Bio = "A marketing strategist with a strong background in digital advertising.",
                    UserName = "charlotte.anderson@avancira.com",
                    Email = "charlotte.anderson@avancira.com",
                    Address = new Address
                    {
                        StreetAddress = "88 Hay St",
                        City = "Perth",
                        State = "WA",
                        Country = "Australia",
                        PostalCode = "6000",
                        Latitude = -31.9535,
                        Longitude = 115.8575,
                        FormattedAddress = "88 Hay St, Perth WA 6000, Australia"
                    },
                    CountryId = context.Countries.FirstOrDefault(c => EF.Functions.Like(c.Code, "AU"))?.Id ?? null,
                    TimeZoneId = "Australia/Perth",
                    ProfileImageUrl = "assets/img/mentor/charlotte_anderson.jpg"
                },
                new User
                {
                    FirstName = "Ethan",
                    LastName = "Brown",
                    Bio = "A robotics engineer with a passion for AI and automation.",
                    UserName = "ethan.brown@avancira.com",
                    Email = "ethan.brown@avancira.com",
                    Address = new Address
                    {
                        StreetAddress = "10 Martin Pl",
                        City = "Sydney",
                        State = "NSW",
                        Country = "Australia",
                        PostalCode = "2000",
                        Latitude = -33.8688,
                        Longitude = 151.2093,
                        FormattedAddress = "10 Martin Pl, Sydney NSW 2000, Australia"
                    },
                    CountryId = context.Countries.FirstOrDefault(c => EF.Functions.Like(c.Code, "AU"))?.Id ?? null,
                    TimeZoneId = "Australia/Sydney",
                    ProfileImageUrl = "assets/img/mentor/ethan_brown.jpg"
                },
                new User
                {
                    FirstName = "Emma",
                    LastName = "Taylor",
                    Bio = "A UX designer with a focus on creating accessible web applications.",
                    UserName = "emma.taylor@avancira.com",
                    Email = "emma.taylor@avancira.com",
                    Address = new Address
                    {
                        StreetAddress = "33 Queen St",
                        City = "Brisbane",
                        State = "QLD",
                        Country = "Australia",
                        PostalCode = "4000",
                        Latitude = -27.4723,
                        Longitude = 153.0287,
                        FormattedAddress = "33 Queen St, Brisbane QLD 4000, Australia"
                    },
                    CountryId = context.Countries.FirstOrDefault(c => EF.Functions.Like(c.Code, "AU"))?.Id ?? null,
                    TimeZoneId = "Australia/Brisbane",
                    ProfileImageUrl = "assets/img/mentor/emma_taylor.jpg"
                },
                new User
                {
                    FirstName = "Michael",
                    LastName = "Harris",
                    Bio = "A data analyst with expertise in big data and machine learning.",
                    UserName = "michael.harris@avancira.com",
                    Email = "michael.harris@avancira.com",
                    Address = new Address
                    {
                        StreetAddress = "12 Hay St",
                        City = "Perth",
                        State = "WA",
                        Country = "Australia",
                        PostalCode = "6000",
                        Latitude = -31.9535,
                        Longitude = 115.8575,
                        FormattedAddress = "12 Hay St, Perth WA 6000, Australia"
                    },
                    CountryId = context.Countries.FirstOrDefault(c => EF.Functions.Like(c.Code, "AU"))?.Id ?? null,
                    TimeZoneId = "Australia/Perth",
                    ProfileImageUrl = "assets/img/mentor/michael_harris.jpg"
                },
                new User
                {
                    FirstName = "Sophia",
                    LastName = "White",
                    Bio = "A high school teacher specializing in mathematics and physics.",
                    UserName = "sophia.white@avancira.com",
                    Email = "sophia.white@avancira.com",
                    Address = new Address
                    {
                        StreetAddress = "55 King St",
                        City = "Sydney",
                        State = "NSW",
                        Country = "Australia",
                        PostalCode = "2000",
                        Latitude = -33.8688,
                        Longitude = 151.2093,
                        FormattedAddress = "55 King St, Sydney NSW 2000, Australia"
                    },
                    CountryId = context.Countries.FirstOrDefault(c => EF.Functions.Like(c.Code, "AU"))?.Id ?? null,
                    TimeZoneId = "Australia/Sydney",
                    ProfileImageUrl = "assets/img/mentor/sophia_white.jpg"
                },
                new User
                {
                    FirstName = "Mei",
                    LastName = "Wong",
                    Bio = "A linguist and translator specializing in Japanese and English languages.",
                    UserName = "mei.wong@avancira.com",
                    Email = "mei.wong@avancira.com",
                    Address = new Address
                    {
                        StreetAddress = "88 Oxford St",
                        City = "Sydney",
                        State = "NSW",
                        Country = "Australia",
                        PostalCode = "2000",
                        Latitude = -33.8688,
                        Longitude = 151.2093,
                        FormattedAddress = "88 Oxford St, Sydney NSW 2000, Australia"
                    },
                    CountryId = context.Countries.FirstOrDefault(c => EF.Functions.Like(c.Code, "AU"))?.Id ?? null,
                    TimeZoneId = "Australia/Sydney",
                    ProfileImageUrl = "assets/img/mentor/mei_wong.jpg"
                }

            };

            foreach (var user in users)
            {
                userManager.CreateAsync(user, "Test@1234").GetAwaiter().GetResult();
            }
        }
    }
}









public class LessonCategorySeeder
{
    public static void Seed(AvanciraDbContext context, UserManager<User> userManager)
    {
        if (!context.LessonCategories.Any())
        {
            var categories = new List<LessonCategory>
            {
                // Academics
                new LessonCategory { Name = "Maths"},
                new LessonCategory { Name = "Physics"},
                new LessonCategory { Name = "Chemistry" },
                new LessonCategory { Name = "Biology" },
                new LessonCategory { Name = "Science" },
                new LessonCategory { Name = "GAMSAT" },
                new LessonCategory { Name = "IELTS"},
                new LessonCategory { Name = "UCAT" },
                new LessonCategory { Name = "ESL" },
                new LessonCategory { Name = "Research", ImageUrl = $"assets/img/categories/cate-16.png", DisplayInLandingPage = true },
                new LessonCategory { Name = "Writing", ImageUrl = $"assets/img/categories/cate-8.png", DisplayInLandingPage = true },

                // Business & Marketing
                new LessonCategory { Name = "Business", ImageUrl = $"assets/img/categories/cate-9.png", DisplayInLandingPage = true },
                new LessonCategory { Name = "Marketing", ImageUrl = $"assets/img/categories/cate-10.png", DisplayInLandingPage = true },
                new LessonCategory { Name = "Finance", ImageUrl = $"assets/img/categories/cate-17.png", DisplayInLandingPage = true },
                new LessonCategory { Name = "Entrepreneurship" },
                new LessonCategory { Name = "Social Media", ImageUrl = $"assets/img/categories/cate-13.png", DisplayInLandingPage = true },
                new LessonCategory { Name = "Public Relations" },
                new LessonCategory { Name = "Advertising" },
                new LessonCategory { Name = "Management" },
                new LessonCategory { Name = "Business Analytics", ImageUrl = $"assets/img/categories/cate-22.png", DisplayInLandingPage = true },
                new LessonCategory { Name = "Market Research", ImageUrl = $"assets/img/categories/cate-21.png", DisplayInLandingPage = true },

                // Languages
                new LessonCategory { Name = "Languages"},
                new LessonCategory { Name = "English"},
                new LessonCategory { Name = "Spanish"},
                new LessonCategory { Name = "French" },
                new LessonCategory { Name = "Japanese" },
                new LessonCategory { Name = "Chinese" },
                new LessonCategory { Name = "Mandarin" },
                new LessonCategory { Name = "Italian" },
                new LessonCategory { Name = "Korean" },
                new LessonCategory { Name = "German" },
                new LessonCategory { Name = "Arabic" },
                new LessonCategory { Name = "Vietnamese" },
                new LessonCategory { Name = "Russian" },
                new LessonCategory { Name = "Indonesian" },

                // Music
                new LessonCategory { Name = "Piano"},
                new LessonCategory { Name = "Guitar"},
                new LessonCategory { Name = "Acoustic Guitar" },
                new LessonCategory { Name = "Bass Guitar" },
                new LessonCategory { Name = "Violin" },
                new LessonCategory { Name = "Cello" },
                new LessonCategory { Name = "Drumming" },
                new LessonCategory { Name = "Singing" },
                new LessonCategory { Name = "Vocal coach" },
                new LessonCategory { Name = "Art" },
                new LessonCategory { Name = "Drawing" },

                // Sports and Fitness
                new LessonCategory { Name = "Swimming"},
                new LessonCategory { Name = "Tennis" },
                new LessonCategory { Name = "Soccer" },
                new LessonCategory { Name = "Basketball" },
                new LessonCategory { Name = "Badminton" },
                new LessonCategory { Name = "Boxing" },
                new LessonCategory { Name = "Surfing" },
                new LessonCategory { Name = "Personal Training" },
                new LessonCategory { Name = "Yoga" },
                new LessonCategory { Name = "Dance" },

                // Professional Development
                new LessonCategory { Name = "Law" },
                new LessonCategory { Name = "Psychology" },
                new LessonCategory { Name = "Public Speaking" },
                new LessonCategory { Name = "Time Management" },
                new LessonCategory { Name = "Investing" },
                new LessonCategory { Name = "Accounting" },
                new LessonCategory { Name = "Business Basics" },

                // Technology
                new LessonCategory { Name = "C++"},
                new LessonCategory { Name = "Python"},
                new LessonCategory { Name = "Java" },
                new LessonCategory { Name = "Crypto Currency" },
                new LessonCategory { Name = "Blockchain" },
                new LessonCategory { Name = "Data Science"},
                new LessonCategory { Name = "Machine Learning"},
                new LessonCategory { Name = "Frontend Development"},
                new LessonCategory { Name = "Backend Development", ImageUrl = $"assets/img/categories/cate-26.png", DisplayInLandingPage = true},
                new LessonCategory { Name = "AWS"},
                new LessonCategory { Name = "Computer Science"},
                new LessonCategory { Name = "Cloud Computing", ImageUrl = $"assets/img/categories/cate-11.png", DisplayInLandingPage = true },
                new LessonCategory { Name = "Web Development", ImageUrl = $"assets/img/categories/cate-12.png", DisplayInLandingPage = true },
                new LessonCategory { Name = "Cybersecurity", ImageUrl = $"assets/img/categories/cate-15.png", DisplayInLandingPage = true },
                new LessonCategory { Name = "Graphic Design", ImageUrl = $"assets/img/categories/cate-7.png", DisplayInLandingPage = true },

                // Hobbies and Leisure
                new LessonCategory { Name = "Photography", ImageUrl = $"assets/img/categories/cate-14.png", DisplayInLandingPage = true },
                new LessonCategory { Name = "Music" },
                new LessonCategory { Name = "Video Editing" },
                new LessonCategory { Name = "Chess" },
                new LessonCategory { Name = "Sewing" },
                new LessonCategory { Name = "Knitting" },
                new LessonCategory { Name = "Origami" },
                new LessonCategory { Name = "Pottery" },
                new LessonCategory { Name = "Woodworking" },
                new LessonCategory { Name = "Cooking" },
                new LessonCategory { Name = "Gardening" },

                // Performing Arts
                new LessonCategory { Name = "Acting" },
                new LessonCategory { Name = "Contemporary Dance" },
                new LessonCategory { Name = "Ballet" },
                new LessonCategory { Name = "Hip Hop Dance" },
                new LessonCategory { Name = "Playwriting" },
                new LessonCategory { Name = "Directing" },

                // Religion and Spirituality
                // Islamic Studies
                new LessonCategory { Name = "Quran" },
                new LessonCategory { Name = "Islamic Studies" },
                new LessonCategory { Name = "Hadith" },
                new LessonCategory { Name = "Tafsir (Quran Interpretation)" },
                new LessonCategory { Name = "Fiqh (Islamic Jurisprudence)" },
                new LessonCategory { Name = "Seerah (Life of Prophet Muhammad)" },
                new LessonCategory { Name = "Islamic History" },

                // Christianity
                new LessonCategory { Name = "Bible Study" },
                new LessonCategory { Name = "Christian Theology" },
                new LessonCategory { Name = "Church History" },
                new LessonCategory { Name = "Ethics in Christianity" },
                new LessonCategory { Name = "Christian Apologetics" },

                // Judaism
                new LessonCategory { Name = "Torah Study" },
                new LessonCategory { Name = "Talmud" },
                new LessonCategory { Name = "Jewish Ethics" },
                new LessonCategory { Name = "Jewish History" },
                new LessonCategory { Name = "Hebrew Language" },

                // Hinduism
                new LessonCategory { Name = "Bhagavad Gita" },
                new LessonCategory { Name = "Vedas and Upanishads" },
                new LessonCategory { Name = "Yoga Philosophy" },
                new LessonCategory { Name = "Puja and Rituals" },

                // Buddhism
                new LessonCategory { Name = "Meditation Practices" },
                new LessonCategory { Name = "Buddhist Philosophy" },
                new LessonCategory { Name = "The Four Noble Truths" },
                new LessonCategory { Name = "History of Buddhism" },

                // Other Religions and Spiritual Practices
                new LessonCategory { Name = "Sikhism Studies" },
                new LessonCategory { Name = "Taoism" },
                new LessonCategory { Name = "Confucianism" },
                new LessonCategory { Name = "Spirituality and Mindfulness" },
                new LessonCategory { Name = "Comparative Religion" }

            };

            context.LessonCategories.AddRange(categories);
            context.SaveChanges();
        }
    }
}




public class ListingSeeder
{
    public static void Seed(AvanciraDbContext context, UserManager<User> userManager)
    {
        if (!context.Listings.Any())
        {
            var listings = new List<Listing>
            {
                new Listing
                {
                    UserId = context.Users.FirstOrDefault(c => EF.Functions.Like(c.Email, "Amr.Mostafa@live.com"))?.Id ?? string.Empty,
                    // LessonCategoryId = context.LessonCategories.FirstOrDefault(c => EF.Functions.Like(c.Name, "C++"))?.Id ?? 0,
                    HourlyRate = 50.0M,
                    Name = "Advanced Programming Lessons (C++)",
                    Description = "Master programming concepts with hands-on lessons in C++ for beginners to advanced levels.",
                    //ListingImagePath = $"assets/img/mentor/amr_mostafa.jpg",
                    Locations = ListingLocationType.Webcam | ListingLocationType.StudentLocation,
                    //AboutYou = "My name is Amr, and I’m a software engineer with years of experience in developing complex systems.",
                    //AboutLesson = "Lessons focus on teaching C++ programming concepts with practical examples and projects.",
                },
                new Listing
                {
                    UserId = context.Users.FirstOrDefault(c => EF.Functions.Like(c.Email, "Amr.Mostafa@live.com"))?.Id ?? string.Empty,
                    // LessonCategoryId = context.LessonCategories.FirstOrDefault(c => EF.Functions.Like(c.Name, "AWS"))?.Id ?? 0,
                    HourlyRate = 60.0M,
                    //Rates = new ListingRates() { Hourly = 60.0M, FiveHours = 60.0M * 5, TenHours = 60.0M * 10 },
                    Name = "AWS and DevOps Fundamentals",
                    Description = "Learn AWS services (EC2, S3, RDS) and DevOps pipelines with tools like Jenkins and Docker.",
                    //ListingImagePath = $"assets/img/mentor/amr_mostafa.jpg",
                    Locations = ListingLocationType.Webcam | ListingLocationType.TutorLocation,
                    //AboutYou = "My name is Amr, and I have extensive experience in cloud infrastructure and DevOps engineering.",
                    //AboutLesson = "Lessons include setting up AWS services, CI/CD pipelines, and hands-on labs for deployment.",
                },
                new Listing
                {
                    UserId = context.Users.FirstOrDefault(c => EF.Functions.Like(c.Email, "Amr.Mostafa@live.com"))?.Id ?? string.Empty,
                    // LessonCategoryId = context.LessonCategories.FirstOrDefault(c => EF.Functions.Like(c.Name, "Machine Learning"))?.Id ?? 0,
                    HourlyRate = 70.0M,
                    //Rates = new ListingRates() { Hourly = 70.0M, FiveHours = 70.0M * 5, TenHours = 70.0M * 10 },
                    Name = "Introduction to Machine Learning",
                    Description = "Build foundational skills in machine learning, focusing on neural networks and deep learning with PyTorch.",
                    //ListingImagePath = $"assets/img/mentor/amr_mostafa.jpg",
                    Locations = ListingLocationType.Webcam | ListingLocationType.StudentLocation,
                    //AboutYou = "I’m Amr, an experienced software engineer specializing in machine learning and AI technologies.",
                    //AboutLesson = "Lessons cover the basics of machine learning, algorithm development, and practical implementations.",
                },
                new Listing
                {
                    UserId = context.Users.FirstOrDefault(c => EF.Functions.Like(c.Email, "Amr.Mostafa@live.com"))?.Id ?? string.Empty,
                    // LessonCategoryId = context.LessonCategories.FirstOrDefault(c => EF.Functions.Like(c.Name, "Computer Science"))?.Id ?? 0,
                    HourlyRate = 55.0M,
                    //Rates = new ListingRates() { Hourly = 55.0M, FiveHours = 55.0M * 5, TenHours = 55.0M * 10 },
                    Name = "Computer Architecture Tutoring",
                    Description = "Specialized lessons in computer architecture and FPGA design for students and professionals.",
                    //ListingImagePath = $"assets/img/mentor/amr_mostafa.jpg",
                    Locations = ListingLocationType.Webcam | ListingLocationType.StudentLocation | ListingLocationType.TutorLocation,
                    //AboutYou = "I’m Amr, a computer engineer with experience in hardware design and low-level programming.",
                    //AboutLesson = "Lessons focus on understanding computer architecture, FPGA programming, and their applications.",
                },
                new Listing
                {
                    UserId = context.Users.FirstOrDefault(c => EF.Functions.Like(c.Email, "Amr.Mostafa@live.com"))?.Id ?? string.Empty,
                    // LessonCategoryId = context.LessonCategories.FirstOrDefault(c => EF.Functions.Like(c.Name, "Python"))?.Id ?? 0,
                    HourlyRate = 45.0M,
                    //Rates = new ListingRates() { Hourly = 45.0M, FiveHours = 45.0M * 5, TenHours = 45.0M * 10 },
                    Name = "Python for Data Science",
                    Description = "Learn Python programming for data analysis, visualization, and machine learning.",
                    //ListingImagePath = $"assets/img/mentor/amr_mostafa.jpg",
                    Locations = ListingLocationType.Webcam | ListingLocationType.TutorLocation,
                    //AboutYou = "My name is Amr, and I’m a software engineer with expertise in Python for data science and AI.",
                    //AboutLesson = "Lessons include Python basics, libraries like NumPy and Pandas, and practical data science projects.",
                },
                new Listing
                {
                    UserId = context.Users.FirstOrDefault(c => EF.Functions.Like(c.Email, "Amr.Mostafa@live.com"))?.Id ?? string.Empty,
                    // LessonCategoryId = context.LessonCategories.FirstOrDefault(c => EF.Functions.Like(c.Name, "Maths"))?.Id ?? 0,
                    HourlyRate = 40.0M,
                    //Rates = new ListingRates() { Hourly = 40.0M, FiveHours = 40.0M * 5, TenHours = 40.0M * 10 },
                    Name = "Mathematics Tutoring for All Levels",
                    Description = "Enhance your math skills with personalized lessons in algebra, calculus, and geometry for all levels.",
                    //ListingImagePath = $"assets/img/mentor/amr_mostafa.jpg",
                    Locations = ListingLocationType.Webcam | ListingLocationType.StudentLocation | ListingLocationType.TutorLocation,
                    //AboutYou = "My name is Amr, and I’m passionate about teaching mathematics to help students achieve their academic goals.",
                    //AboutLesson = "Lessons cover fundamental to advanced mathematical concepts with real-world problem-solving techniques.",
                },
                new Listing
                {
                    UserId = context.Users.FirstOrDefault(c => EF.Functions.Like(c.Email, "Amr.Mostafa@live.com"))?.Id ?? string.Empty,
                    // LessonCategoryId = context.LessonCategories.FirstOrDefault(c => EF.Functions.Like(c.Name, "Java"))?.Id ?? 0,
                    HourlyRate = 55.0M,
                    //Rates = new ListingRates() { Hourly = 55.0M, FiveHours = 55.0M * 5, TenHours = 55.0M * 10 },
                    Name = "Java Programming Essentials",
                    Description = "Learn Java programming from basics to advanced, focusing on object-oriented programming and real-world applications.",
                    //ListingImagePath = $"assets/img/mentor/amr_mostafa.jpg",
                    Locations = ListingLocationType.Webcam | ListingLocationType.StudentLocation | ListingLocationType.TutorLocation,
                    //AboutYou = "My name is Amr, and I have experience teaching Java to professionals and students alike.",
                    //AboutLesson = "Lessons cover Java syntax, OOP concepts, and building Java-based applications.",
                },
                // Frontend Development - Angular
                new Listing
                {
                    UserId = context.Users.FirstOrDefault(c => EF.Functions.Like(c.Email, "Amr.Mostafa@live.com"))?.Id ?? string.Empty,
                    // LessonCategoryId = context.LessonCategories.FirstOrDefault(c => EF.Functions.Like(c.Name, "Frontend Development"))?.Id ?? 0,
                    HourlyRate = 55.0M,
                    //Rates = new ListingRates() { Hourly = 55.0M, FiveHours = 55.0M * 5, TenHours = 55.0M * 10 },
                    Name = "Frontend Development with Angular",
                    Description = "Learn how to build responsive web applications using Angular.",
                    //ListingImagePath = $"assets/img/mentor/amr_mostafa.jpg",
                    Locations = ListingLocationType.Webcam | ListingLocationType.TutorLocation,
                    //AboutYou = "My name is Amr, and I specialize in frontend development using modern frameworks like Angular.",
                    //AboutLesson = "Lessons include TypeScript basics, Angular component architecture, and building real-world applications.",
                },

                // Frontend Development - React
                new Listing
                {
                    UserId = context.Users.FirstOrDefault(c => EF.Functions.Like(c.Email, "Amr.Mostafa@live.com"))?.Id ?? string.Empty,
                    // LessonCategoryId = context.LessonCategories.FirstOrDefault(c => EF.Functions.Like(c.Name, "Frontend Development"))?.Id ?? 0,
                    HourlyRate = 55.0M,
                    //Rates = new ListingRates() { Hourly = 55.0M, FiveHours = 55.0M * 5, TenHours = 55.0M * 10 },
                    Name = "Frontend Development with React",
                    Description = "Learn how to create dynamic and interactive web applications using React.",
                    //ListingImagePath = $"assets/img/mentor/amr_mostafa.jpg",
                    Locations = ListingLocationType.Webcam | ListingLocationType.TutorLocation,
                    //AboutYou = "My name is Amr, and I have extensive experience in building scalable React applications.",
                    //AboutLesson = "Lessons include React basics, state management with Redux, and deploying React apps.",
                },

                // Backend Development - .NET
                new Listing
                {
                    UserId = context.Users.FirstOrDefault(c => EF.Functions.Like(c.Email, "Amr.Mostafa@live.com"))?.Id ?? string.Empty,
                    // LessonCategoryId = context.LessonCategories.FirstOrDefault(c => EF.Functions.Like(c.Name, "Backend Development"))?.Id ?? 0,
                    HourlyRate = 65.0M,
                    //Rates = new ListingRates() { Hourly = 65.0M, FiveHours = 65.0M * 5, TenHours = 65.0M * 10 },
                    Name = "Backend Development with .NET",
                    Description = "Master backend development using .NET and build robust APIs.",
                    //ListingImagePath = $"assets/img/mentor/amr_mostafa.jpg",
                    Locations = ListingLocationType.Webcam | ListingLocationType.StudentLocation | ListingLocationType.TutorLocation,
                    //AboutYou = "My name is Amr, and I have deep expertise in .NET for backend development.",
                    //AboutLesson = "Lessons include .NET Core basics, RESTful API design, and database integration with Entity Framework.",
                },

                // Backend Development - Advanced .NET
                new Listing
                {
                    UserId = context.Users.FirstOrDefault(c => EF.Functions.Like(c.Email, "Amr.Mostafa@live.com"))?.Id ?? string.Empty,
                    // LessonCategoryId = context.LessonCategories.FirstOrDefault(c => EF.Functions.Like(c.Name, "Backend Development"))?.Id ?? 0,
                    HourlyRate = 75.0M,
                    //Rates = new ListingRates() { Hourly = 75.0M, FiveHours = 75.0M * 5, TenHours = 75.0M * 10 },
                    Name = "Advanced Backend Development with .NET",
                    Description = "Dive deeper into advanced .NET features and build enterprise-grade applications.",
                    //ListingImagePath = $"assets/img/mentor/amr_mostafa.jpg",
                    Locations = ListingLocationType.Webcam | ListingLocationType.TutorLocation,
                    //AboutYou = "I’m Amr, and I have years of experience building enterprise solutions using .NET technologies.",
                    //AboutLesson = "Lessons cover advanced .NET topics like dependency injection, authentication, and microservices architecture.",
                },
                // Cloud Computing
                new Listing
                {
                    UserId = context.Users.FirstOrDefault(c => EF.Functions.Like(c.Email, "Amr.Mostafa@live.com"))?.Id ?? string.Empty,
                    // LessonCategoryId = context.LessonCategories.FirstOrDefault(c => EF.Functions.Like(c.Name, "Cloud Computing"))?.Id ?? 0,
                    HourlyRate = 70.0M,
                    //Rates = new ListingRates() { Hourly = 70.0M, FiveHours = 70.0M * 5, TenHours = 70.0M * 10 },
                    Name = "AWS Solutions Architect Masterclass",
                    Description = "Prepare for the AWS Solutions Architect certification with hands-on labs in EC2, S3, VPC, and networking.",
                    //ListingImagePath = $"assets/img/mentor/amr_mostafa.jpg",
                    Locations = ListingLocationType.Webcam | ListingLocationType.StudentLocation,
                    //AboutYou = "I’m Amr, a cloud engineer with experience in AWS and DevOps.",
                    //AboutLesson = "Covers AWS design principles, cost optimization, security best practices, and case studies.",
                },
                new Listing
                {
                    UserId = context.Users.FirstOrDefault(c => EF.Functions.Like(c.Email, "Amr.Mostafa@live.com"))?.Id ?? string.Empty,
                    // LessonCategoryId = context.LessonCategories.FirstOrDefault(c => EF.Functions.Like(c.Name, "Cloud Computing"))?.Id ?? 0,
                    HourlyRate = 60.0M,
                    //Rates = new ListingRates() { Hourly = 60.0M, FiveHours = 60.0M * 5, TenHours = 60.0M * 10 },
                    Name = "Google Cloud Platform for Developers",
                    Description = "Learn GCP services like Compute Engine, BigQuery, and Cloud Functions.",
                    //ListingImagePath = $"assets/img/mentor/amr_mostafa.jpg",
                    Locations = ListingLocationType.Webcam | ListingLocationType.TutorLocation,
                    //AboutYou = "My name is Amr, and I specialize in cloud platforms like AWS, GCP, and Azure.",
                    //AboutLesson = "Includes hands-on experience in deploying applications on GCP.",
                },

                // Web Development
                new Listing
                {
                    UserId = context.Users.FirstOrDefault(c => EF.Functions.Like(c.Email, "Amr.Mostafa@live.com"))?.Id ?? string.Empty,
                    // LessonCategoryId = context.LessonCategories.FirstOrDefault(c => EF.Functions.Like(c.Name, "Web Development"))?.Id ?? 0,
                    HourlyRate = 60.0M,
                    //Rates = new ListingRates() { Hourly = 60.0M, FiveHours = 60.0M * 5, TenHours = 60.0M * 10 },
                    Name = "Full-Stack Web Development with MERN",
                    Description = "Learn to build full-stack web applications using MongoDB, Express, React, and Node.js.",
                    //ListingImagePath = $"assets/img/mentor/amr_mostafa.jpg",
                    Locations = ListingLocationType.Webcam | ListingLocationType.TutorLocation,
                    //AboutYou = "My name is Amr, and I specialize in full-stack web development.",
                    //AboutLesson = "Covers front-end and back-end development with RESTful APIs.",
                },

                // Cybersecurity
                new Listing
                {
                    UserId = context.Users.FirstOrDefault(c => EF.Functions.Like(c.Email, "Amr.Mostafa@live.com"))?.Id ?? string.Empty,
                    // LessonCategoryId = context.LessonCategories.FirstOrDefault(c => EF.Functions.Like(c.Name, "Cybersecurity"))?.Id ?? 0,
                    HourlyRate = 75.0M,
                    //Rates = new ListingRates() { Hourly = 75.0M, FiveHours = 75.0M * 5, TenHours = 75.0M * 10 },
                    Name = "Ethical Hacking with Kali Linux",
                    Description = "Learn ethical hacking, penetration testing, and network security.",
                    //ListingImagePath = $"assets/img/mentor/amr_mostafa.jpg",
                    Locations = ListingLocationType.Webcam | ListingLocationType.StudentLocation,
                    //AboutYou = "I’m Amr, a cybersecurity specialist with hands-on penetration testing experience.",
                    //AboutLesson = "Covers penetration testing, vulnerability scanning, and security frameworks.",
                },

                // Graphic Design
                new Listing
                {
                    UserId = context.Users.FirstOrDefault(c => EF.Functions.Like(c.Email, "Amr.Mostafa@live.com"))?.Id ?? string.Empty,
                    // LessonCategoryId = context.LessonCategories.FirstOrDefault(c => EF.Functions.Like(c.Name, "Graphic Design"))?.Id ?? 0,
                    HourlyRate = 50.0M,
                    //Rates = new ListingRates() { Hourly = 50.0M, FiveHours = 50.0M * 5, TenHours = 50.0M * 10 },
                    Name = "Mastering Photoshop & Illustrator for Designers",
                    Description = "Learn photo editing, vector graphics, and branding.",
                    //ListingImagePath = $"assets/img/mentor/amr_mostafa.jpg",
                    Locations = ListingLocationType.Webcam | ListingLocationType.TutorLocation,
                    //AboutYou = "My name is Amr, and I’m a professional designer with experience in Adobe Creative Suite.",
                    //AboutLesson = "Covers professional design techniques and real-world projects.",
                },






                // =================== Business Listings for Amir.Salah ===================
                // Finance
                new Listing
                {
                    UserId = context.Users.FirstOrDefault(c => EF.Functions.Like(c.Email, "Amir.Salah@live.com"))?.Id ?? string.Empty,
                    // LessonCategoryId = context.LessonCategories.FirstOrDefault(c => EF.Functions.Like(c.Name, "Finance"))?.Id ?? 0,
                    HourlyRate = 70.0M,
                    //Rates = new ListingRates() { Hourly = 70.0M, FiveHours = 70.0M * 5, TenHours = 70.0M * 10 },
                    Name = "Financial Planning & Investment Strategies",
                    Description = "Learn to manage personal and business finances, investment strategies, and risk assessment.",
                    //ListingImagePath = $"assets/img/mentor/amir_salah.jpg",
                    Locations = ListingLocationType.Webcam | ListingLocationType.StudentLocation,
                    //AboutYou = "I’m Amir, a finance professional with years of experience in investment strategies.",
                    //AboutLesson = "Covers financial planning, stock market basics, and investment management.",
                },

                new Listing
                {
                    UserId = context.Users.FirstOrDefault(c => EF.Functions.Like(c.Email, "Amir.Salah@live.com"))?.Id ?? string.Empty,
                    // LessonCategoryId = context.LessonCategories.FirstOrDefault(c => EF.Functions.Like(c.Name, "Finance"))?.Id ?? 0,
                    HourlyRate = 75.0M,
                    //Rates = new ListingRates() { Hourly = 75.0M, FiveHours = 75.0M * 5, TenHours = 75.0M * 10 },
                    Name = "Corporate Finance & Accounting",
                    Description = "Learn how businesses manage financial decisions, budgeting, and cash flow.",
                    //ListingImagePath = $"assets/img/mentor/amir_salah.jpg",
                    Locations = ListingLocationType.Webcam | ListingLocationType.TutorLocation,
                    //AboutYou = "I’m Amir, an expert in corporate finance, accounting, and financial modeling.",
                    //AboutLesson = "Focuses on budgeting, cost management, and corporate financial strategies.",
                },

                // Marketing
                new Listing
                {
                    UserId = context.Users.FirstOrDefault(c => EF.Functions.Like(c.Email, "Amir.Salah@live.com"))?.Id ?? string.Empty,
                    // LessonCategoryId = context.LessonCategories.FirstOrDefault(c => EF.Functions.Like(c.Name, "Marketing"))?.Id ?? 0,
                    HourlyRate = 65.0M,
                    //Rates = new ListingRates() { Hourly = 65.0M, FiveHours = 65.0M * 5, TenHours = 65.0M * 10 },
                    Name = "Digital Marketing & Social Media Strategy",
                    Description = "Learn SEO, paid ads, social media growth, and digital branding.",
                    //ListingImagePath = $"assets/img/mentor/amir_salah.jpg",
                    Locations = ListingLocationType.Webcam | ListingLocationType.TutorLocation,
                    //AboutYou = "My name is Amir, and I specialize in digital marketing and brand strategy.",
                    //AboutLesson = "Includes hands-on experience in running ad campaigns and optimizing content marketing.",
                },

                // Business Analytics
                new Listing
                {
                    UserId = context.Users.FirstOrDefault(c => EF.Functions.Like(c.Email, "Amir.Salah@live.com"))?.Id ?? string.Empty,
                    // LessonCategoryId = context.LessonCategories.FirstOrDefault(c => EF.Functions.Like(c.Name, "Business Analytics"))?.Id ?? 0,
                    HourlyRate = 80.0M,
                    //Rates = new ListingRates() { Hourly = 80.0M, FiveHours = 80.0M * 5, TenHours = 80.0M * 10 },
                    Name = "Business Analytics with Excel & Power BI",
                    Description = "Learn how to analyze business data using Excel, SQL, and Power BI dashboards.",
                    //ListingImagePath = $"assets/img/mentor/amir_salah.jpg",
                    Locations = ListingLocationType.Webcam | ListingLocationType.StudentLocation,
                    //AboutYou = "I’m Amir, a business analyst with experience in data visualization and analytics.",
                    //AboutLesson = "Covers business intelligence, dashboard creation, and data-driven decision making.",
                },

                new Listing
                {
                    UserId = context.Users.FirstOrDefault(c => EF.Functions.Like(c.Email, "Amir.Salah@live.com"))?.Id ?? string.Empty,
                    // LessonCategoryId = context.LessonCategories.FirstOrDefault(c => EF.Functions.Like(c.Name, "Business Analytics"))?.Id ?? 0,
                    HourlyRate = 85.0M,
                    //Rates = new ListingRates() { Hourly = 85.0M, FiveHours = 85.0M * 5, TenHours = 85.0M * 10 },
                    Name = "Data Science & Business Forecasting",
                    Description = "Use predictive analytics and machine learning for business decision-making.",
                    //ListingImagePath = $"assets/img/mentor/amir_salah.jpg",
                    Locations = ListingLocationType.Webcam | ListingLocationType.TutorLocation,
                    //AboutYou = "My name is Amir, and I help businesses leverage data to drive growth.",
                    //AboutLesson = "Focuses on business forecasting, statistical modeling, and machine learning applications.",
                },

                // Management
                new Listing
                {
                    UserId = context.Users.FirstOrDefault(c => EF.Functions.Like(c.Email, "Amir.Salah@live.com"))?.Id ?? string.Empty,
                    // LessonCategoryId = context.LessonCategories.FirstOrDefault(c => EF.Functions.Like(c.Name, "Management"))?.Id ?? 0,
                    HourlyRate = 60.0M,
                    //Rates = new ListingRates() { Hourly = 60.0M, FiveHours = 60.0M * 5, TenHours = 60.0M * 10 },
                    Name = "Leadership & Management Skills",
                    Description = "Learn effective leadership, communication, and team management strategies.",
                    //ListingImagePath = $"assets/img/mentor/amir_salah.jpg",
                    Locations = ListingLocationType.Webcam | ListingLocationType.StudentLocation,
                    //AboutYou = "I’m Amir, an experienced business coach specializing in leadership and management.",
                    //AboutLesson = "Covers strategic leadership, decision-making, and organizational management.",
                },

                new Listing
                {
                    UserId = context.Users.FirstOrDefault(c => EF.Functions.Like(c.Email, "Amir.Salah@live.com"))?.Id ?? string.Empty,
                    // LessonCategoryId = context.LessonCategories.FirstOrDefault(c => EF.Functions.Like(c.Name, "Management"))?.Id ?? 0,
                    HourlyRate = 65.0M,
                    //Rates = new ListingRates() { Hourly = 65.0M, FiveHours = 65.0M * 5, TenHours = 65.0M * 10 },
                    Name = "Project Management & Agile Methodologies",
                    Description = "Learn project management principles and Agile methodologies like Scrum and Kanban.",
                    //ListingImagePath = $"assets/img/mentor/amir_salah.jpg",
                    Locations = ListingLocationType.Webcam | ListingLocationType.TutorLocation,
                    //AboutYou = "I’m Amir, a certified project manager with extensive experience in Agile methodologies.",
                    //AboutLesson = "Covers project planning, Agile principles, and real-world case studies.",
                },



                // =================== Creative Listings for Ahmed.Mostafa ===================

                // Photography
                new Listing
                {
                    UserId = context.Users.FirstOrDefault(c => EF.Functions.Like(c.Email, "Ahmed.Mostafa@live.com"))?.Id ?? string.Empty,
                    // LessonCategoryId = context.LessonCategories.FirstOrDefault(c => EF.Functions.Like(c.Name, "Photography"))?.Id ?? 0,
                    HourlyRate = 55.0M,
                    //Rates = new ListingRates() { Hourly = 55.0M, FiveHours = 55.0M * 5, TenHours = 55.0M * 10 },
                    Name = "Photography Masterclass: From Beginner to Pro",
                    Description = "Learn the fundamentals of photography, camera settings, lighting, and composition.",
                    //ListingImagePath = $"assets/img/mentor/ahmed_mostafa.jpg",
                    Locations = ListingLocationType.Webcam | ListingLocationType.StudentLocation,
                    //AboutYou = "I’m Ahmed, a professional photographer with years of experience in portrait and landscape photography.",
                    //AboutLesson = "Covers DSLR settings, lighting techniques, and post-processing skills in Adobe Lightroom.",
                },

                new Listing
                {
                    UserId = context.Users.FirstOrDefault(c => EF.Functions.Like(c.Email, "Ahmed.Mostafa@live.com"))?.Id ?? string.Empty,
                    // LessonCategoryId = context.LessonCategories.FirstOrDefault(c => EF.Functions.Like(c.Name, "Photography"))?.Id ?? 0,
                    HourlyRate = 60.0M,
                    //Rates = new ListingRates() { Hourly = 60.0M, FiveHours = 60.0M * 5, TenHours = 60.0M * 10 },
                    Name = "Advanced Editing Techniques in Lightroom & Photoshop",
                    Description = "Master professional photo editing and retouching using Lightroom and Photoshop.",
                    //ListingImagePath = $"assets/img/mentor/ahmed_mostafa.jpg",
                    Locations = ListingLocationType.Webcam | ListingLocationType.TutorLocation,
                    //AboutYou = "I’m Ahmed, an expert in digital editing with experience in magazine and commercial photography.",
                    //AboutLesson = "Covers retouching, color grading, and creating stunning visual effects.",
                },

                // Writing & Content Creation
                new Listing
                {
                    UserId = context.Users.FirstOrDefault(c => EF.Functions.Like(c.Email, "Ahmed.Mostafa@live.com"))?.Id ?? string.Empty,
                    // LessonCategoryId = context.LessonCategories.FirstOrDefault(c => EF.Functions.Like(c.Name, "Writing"))?.Id ?? 0,
                    HourlyRate = 50.0M,
                    //Rates = new ListingRates() { Hourly = 50.0M, FiveHours = 50.0M * 5, TenHours = 50.0M * 10 },
                    Name = "Creative Writing & Blogging Essentials",
                    Description = "Learn how to craft compelling stories, blog posts, and engaging content.",
                    //ListingImagePath = $"assets/img/mentor/ahmed_mostafa.jpg",
                    Locations = ListingLocationType.Webcam | ListingLocationType.StudentLocation,
                    //AboutYou = "My name is Ahmed, and I’ve worked as a content writer for various online platforms.",
                    //AboutLesson = "Focuses on writing techniques, storytelling, and SEO-friendly content.",
                },

                new Listing
                {
                    UserId = context.Users.FirstOrDefault(c => EF.Functions.Like(c.Email, "Ahmed.Mostafa@live.com"))?.Id ?? string.Empty,
                    // LessonCategoryId = context.LessonCategories.FirstOrDefault(c => EF.Functions.Like(c.Name, "Writing"))?.Id ?? 0,
                    HourlyRate = 55.0M,
                    //Rates = new ListingRates() { Hourly = 55.0M, FiveHours = 55.0M * 5, TenHours = 55.0M * 10 },
                    Name = "Freelance Writing & Monetizing Your Content",
                    Description = "Learn how to make money writing articles, blogs, and ebooks.",
                    //ListingImagePath = $"assets/img/mentor/ahmed_mostafa.jpg",
                    Locations = ListingLocationType.Webcam | ListingLocationType.TutorLocation,
                    //AboutYou = "I’m Ahmed, and I help writers turn their passion into a career.",
                    //AboutLesson = "Includes platforms like Medium, Upwork, and strategies for passive income.",
                },

                // Graphic Design
                new Listing
                {
                    UserId = context.Users.FirstOrDefault(c => EF.Functions.Like(c.Email, "Ahmed.Mostafa@live.com"))?.Id ?? string.Empty,
                    // LessonCategoryId = context.LessonCategories.FirstOrDefault(c => EF.Functions.Like(c.Name, "Graphic Design"))?.Id ?? 0,
                    HourlyRate = 60.0M,
                    //Rates = new ListingRates() { Hourly = 60.0M, FiveHours = 60.0M * 5, TenHours = 60.0M * 10 },
                    Name = "Graphic Design with Adobe Illustrator & Photoshop",
                    Description = "Learn how to design logos, branding materials, and social media graphics.",
                    //ListingImagePath = $"assets/img/mentor/ahmed_mostafa.jpg",
                    Locations = ListingLocationType.Webcam | ListingLocationType.TutorLocation,
                    //AboutYou = "My name is Ahmed, and I’m a professional graphic designer with experience in branding and visual identity.",
                    //AboutLesson = "Covers logo design, typography, and creating print-ready designs.",
                },

                // Video Editing
                new Listing
                {
                    UserId = context.Users.FirstOrDefault(c => EF.Functions.Like(c.Email, "Ahmed.Mostafa@live.com"))?.Id ?? string.Empty,
                    // LessonCategoryId = context.LessonCategories.FirstOrDefault(c => EF.Functions.Like(c.Name, "Video Editing"))?.Id ?? 0,
                    HourlyRate = 70.0M,
                    //Rates = new ListingRates() { Hourly = 70.0M, FiveHours = 70.0M * 5, TenHours = 70.0M * 10 },
                    Name = "Video Editing with Adobe Premiere Pro & After Effects",
                    Description = "Learn how to create professional video edits and motion graphics.",
                    //ListingImagePath = $"assets/img/mentor/ahmed_mostafa.jpg",
                    Locations = ListingLocationType.Webcam | ListingLocationType.StudentLocation,
                    //AboutYou = "I’m Ahmed, a professional video editor with years of experience in motion graphics and storytelling.",
                    //AboutLesson = "Covers transitions, effects, animation, and cinematic color grading.",
                },

                new Listing
                {
                    UserId = context.Users.FirstOrDefault(c => EF.Functions.Like(c.Email, "Ahmed.Mostafa@live.com"))?.Id ?? string.Empty,
                    // LessonCategoryId = context.LessonCategories.FirstOrDefault(c => EF.Functions.Like(c.Name, "Video Editing"))?.Id ?? 0,
                    HourlyRate = 75.0M,
                    //Rates = new ListingRates() { Hourly = 75.0M, FiveHours = 75.0M * 5, TenHours = 75.0M * 10 },
                    Name = "YouTube Content Creation & Video Marketing",
                    Description = "Learn how to create engaging YouTube videos and grow your channel.",
                    //ListingImagePath = $"assets/img/mentor/ahmed_mostafa.jpg",
                    Locations = ListingLocationType.Webcam | ListingLocationType.TutorLocation,
                    //AboutYou = "My name is Ahmed, and I help creators build successful YouTube channels.",
                    //AboutLesson = "Includes content strategy, video scripting, and audience growth techniques.",
                },






                new Listing
                {
                    UserId = context.Users.FirstOrDefault(c => EF.Functions.Like(c.Email, "michael.harris@avancira.com"))?.Id ?? string.Empty,
                    // LessonCategoryId = context.LessonCategories.FirstOrDefault(c => EF.Functions.Like(c.Name, "Research"))?.Id ?? 0,
                    HourlyRate = 55.0M,
                    //Rates = new ListingRates() { Hourly = 55.0M, FiveHours = 55.0M * 5, TenHours = 55.0M * 10 },
                    Name = "Data Research and Analytics",
                    Description = "Deep dive into research methodologies and data analytics with expert guidance.",
                    //ListingImagePath = "assets/img/mentor/michael_harris.jpg",
                    Locations = ListingLocationType.Webcam | ListingLocationType.TutorLocation,
                    //AboutYou = "I am Michael, a data analyst specializing in big data and research analytics.",
                    //AboutLesson = "Lessons focus on developing research strategies, data analysis techniques, and tools like Python and R.",
                },
                new Listing
                {
                    UserId = context.Users.FirstOrDefault(c => EF.Functions.Like(c.Email, "michael.harris@avancira.com"))?.Id ?? string.Empty,
                    // LessonCategoryId = context.LessonCategories.FirstOrDefault(c => EF.Functions.Like(c.Name, "Business Analytics"))?.Id ?? 0,
                    HourlyRate = 60.0M,
                    //Rates = new ListingRates() { Hourly = 60.0M, FiveHours = 60.0M * 5, TenHours = 60.0M * 10 },
                    Name = "Business Analytics and Data Science",
                    Description = "Learn business analytics with practical applications in data science.",
                    //ListingImagePath = "assets/img/mentor/michael_harris.jpg",
                    Locations = ListingLocationType.Webcam | ListingLocationType.TutorLocation,
                    //AboutYou = "I am Michael, a data scientist with expertise in business analytics and predictive modeling.",
                    //AboutLesson = "Lessons cover business intelligence tools, data visualization, and predictive analytics.",
                },
                new Listing
                {
                    UserId = context.Users.FirstOrDefault(c => EF.Functions.Like(c.Email, "ava.smith@avancira.com"))?.Id ?? string.Empty,
                    // LessonCategoryId = context.LessonCategories.FirstOrDefault(c => EF.Functions.Like(c.Name, "Business"))?.Id ?? 0,
                    HourlyRate = 60.0M,
                    //Rates = new ListingRates() { Hourly = 60.0M, FiveHours = 60.0M * 5, TenHours = 60.0M * 10 },
                    Name = "Business Strategy and Market Research",
                    Description = "Develop business strategies and market research techniques with real-world applications.",
                    //ListingImagePath = "assets/img/mentor/ava_smith.jpg",
                    Locations = ListingLocationType.Webcam | ListingLocationType.TutorLocation,
                    //AboutYou = "I'm Ava, an experienced business analyst with expertise in strategic planning and market analysis.",
                    //AboutLesson = "Lessons cover business models, case studies, and market research methodologies.",
                },
                new Listing
                {
                    UserId = context.Users.FirstOrDefault(c => EF.Functions.Like(c.Email, "ava.smith@avancira.com"))?.Id ?? string.Empty,
                    // LessonCategoryId = context.LessonCategories.FirstOrDefault(c => EF.Functions.Like(c.Name, "Finance"))?.Id ?? 0,
                    HourlyRate = 65.0M,
                    //Rates = new ListingRates() { Hourly = 65.0M, FiveHours = 65.0M * 5, TenHours = 65.0M * 10 },
                    Name = "Finance and Investment Strategies",
                    Description = "Learn financial planning, investment strategies, and portfolio management.",
                    //ListingImagePath = "assets/img/mentor/ava_smith.jpg",
                    Locations = ListingLocationType.Webcam | ListingLocationType.TutorLocation,
                    //AboutYou = "I'm Ava, a financial expert helping students understand finance and investment principles.",
                    //AboutLesson = "Lessons cover financial markets, risk management, and investment analysis.",
                },
                new Listing
                {
                    UserId = context.Users.FirstOrDefault(c => EF.Functions.Like(c.Email, "ethan.clark@avancira.com"))?.Id ?? string.Empty,
                    // LessonCategoryId = context.LessonCategories.FirstOrDefault(c => EF.Functions.Like(c.Name, "Machine Learning"))?.Id ?? 0,
                    HourlyRate = 70.0M,
                    //Rates = new ListingRates() { Hourly = 70.0M, FiveHours = 70.0M * 5, TenHours = 70.0M * 10 },
                    Name = "Deep Learning and AI Fundamentals",
                    Description = "Learn deep learning concepts and AI fundamentals with hands-on coding exercises.",
                    //ListingImagePath = "assets/img/mentor/ethan_clark.jpg",
                    Locations = ListingLocationType.Webcam | ListingLocationType.StudentLocation,
                    //AboutYou = "I’m Ethan, an AI researcher focusing on deep learning and computer vision applications.",
                    //AboutLesson = "Lessons include neural networks, AI architectures, and hands-on projects using TensorFlow and PyTorch.",
                },
                new Listing
                {
                    UserId = context.Users.FirstOrDefault(c => EF.Functions.Like(c.Email, "ethan.clark@avancira.com"))?.Id ?? string.Empty,
                    // LessonCategoryId = context.LessonCategories.FirstOrDefault(c => EF.Functions.Like(c.Name, "Cybersecurity"))?.Id ?? 0,
                    HourlyRate = 75.0M,
                    //Rates = new ListingRates() { Hourly = 75.0M, FiveHours = 75.0M * 5, TenHours = 75.0M * 10 },
                    Name = "Ethical Hacking and Cybersecurity Basics",
                    Description = "Learn cybersecurity principles and ethical hacking techniques with real-world applications.",
                    //ListingImagePath = "assets/img/mentor/ethan_clark.jpg",
                    Locations = ListingLocationType.Webcam | ListingLocationType.TutorLocation,
                    //AboutYou = "I’m Ethan, an AI researcher also focusing on cybersecurity best practices.",
                    //AboutLesson = "Lessons cover penetration testing, network security, and ethical hacking methodologies.",
                },
                new Listing
                {
                    UserId = context.Users.FirstOrDefault(c => EF.Functions.Like(c.Email, "sophia.white@avancira.com"))?.Id ?? string.Empty,
                    // LessonCategoryId = context.LessonCategories.FirstOrDefault(c => EF.Functions.Like(c.Name, "Writing"))?.Id ?? 0,
                    HourlyRate = 50.0M,
                    //Rates = new ListingRates() { Hourly = 50.0M, FiveHours = 50.0M * 5, TenHours = 50.0M * 10 },
                    Name = "Academic Writing and Research Skills",
                    Description = "Improve your academic writing and research skills with expert guidance.",
                    //ListingImagePath = "assets/img/mentor/sophia_white.jpg",
                    Locations = ListingLocationType.Webcam | ListingLocationType.StudentLocation,
                    //AboutYou = "I'm Sophia, a dedicated educator specializing in academic writing and research methodologies.",
                    //AboutLesson = "Lessons focus on structuring academic papers, conducting effective research, and improving writing clarity.",
                },
                new Listing
                {
                    UserId = context.Users.FirstOrDefault(c => EF.Functions.Like(c.Email, "mei.wong@avancira.com"))?.Id ?? string.Empty,
                    // LessonCategoryId = context.LessonCategories.FirstOrDefault(c => EF.Functions.Like(c.Name, "Languages"))?.Id ?? 0,
                    HourlyRate = 45.0M,
                    //Rates = new ListingRates() { Hourly = 45.0M, FiveHours = 45.0M * 5, TenHours = 45.0M * 10 },
                    Name = "Japanese and English Language Lessons",
                    Description = "Master Japanese and English languages with structured lessons and cultural insights.",
                    //ListingImagePath = "assets/img/mentor/mei_wong.jpg",
                    Locations = ListingLocationType.Webcam | ListingLocationType.TutorLocation,
                    //AboutYou = "I'm Mei, a linguist and translator specializing in Japanese and English languages.",
                    //AboutLesson = "Lessons focus on language fundamentals, pronunciation, and cultural contexts.",
                },
                new Listing
                {
                    UserId = context.Users.FirstOrDefault(c => EF.Functions.Like(c.Email, "noah.davis@avancira.com"))?.Id ?? string.Empty,
                    // LessonCategoryId = context.LessonCategories.FirstOrDefault(c => EF.Functions.Like(c.Name, "Physics"))?.Id ?? 0,
                    HourlyRate = 65.0M,
                    //Rates = new ListingRates() { Hourly = 65.0M, FiveHours = 65.0M * 5, TenHours = 65.0M * 10 },
                    Name = "Physics and Space Science Lessons",
                    Description = "Explore physics concepts and space science fundamentals with a passionate professor.",
                    //ListingImagePath = "assets/img/mentor/noah_davis.jpg",
                    Locations = ListingLocationType.Webcam | ListingLocationType.StudentLocation,
                    //AboutYou = "I'm Noah, a physics professor with a passion for space exploration and astrophysics.",
                    //AboutLesson = "Lessons cover classical physics, modern theories, and practical applications in astronomy.",
                },
                new Listing
                {
                    UserId = context.Users.FirstOrDefault(c => EF.Functions.Like(c.Email, "olivia.brown@avancira.com"))?.Id ?? string.Empty,
                    // LessonCategoryId = context.LessonCategories.FirstOrDefault(c => EF.Functions.Like(c.Name, "Music"))?.Id ?? 0,
                    HourlyRate = 55.0M,
                    //Rates = new ListingRates() { Hourly = 55.0M, FiveHours = 55.0M * 5, TenHours = 55.0M * 10 },
                    Name = "Classical and Contemporary Music Lessons",
                    Description = "Develop musical skills and appreciation with structured lessons in classical and contemporary styles.",
                    //ListingImagePath = "assets/img/mentor/olivia_brown.jpg",
                    Locations = ListingLocationType.Webcam | ListingLocationType.TutorLocation,
                    //AboutYou = "I'm Olivia, a musician and teacher passionate about classical and contemporary music.",
                    //AboutLesson = "Lessons focus on music theory, instrument techniques, and performance strategies.",
                },
                new Listing
                {
                    UserId = context.Users.FirstOrDefault(c => EF.Functions.Like(c.Email, "ava.smith@avancira.com"))?.Id ?? string.Empty,
                    // LessonCategoryId = context.LessonCategories.FirstOrDefault(c => EF.Functions.Like(c.Name, "Market Research"))?.Id ?? 0,
                    HourlyRate = 60.0M,
                    //Rates = new ListingRates() { Hourly = 60.0M, FiveHours = 60.0M * 5, TenHours = 60.0M * 10 },
                    Name = "Market Research and Business Analytics",
                    Description = "Learn market research methodologies and business analytics for strategic decision-making.",
                    //ListingImagePath = "assets/img/mentor/ava_smith.jpg",
                    Locations = ListingLocationType.Webcam | ListingLocationType.TutorLocation,
                    //AboutYou = "I'm Ava, an experienced business analyst specializing in market research and strategy.",
                    //AboutLesson = "Lessons cover competitive analysis, customer insights, and data-driven market strategies.",
                },
                new Listing
                {
                    UserId = context.Users.FirstOrDefault(c => EF.Functions.Like(c.Email, "charlotte.anderson@avancira.com"))?.Id ?? string.Empty,
                    // LessonCategoryId = context.LessonCategories.FirstOrDefault(c => EF.Functions.Like(c.Name, "Social Media"))?.Id ?? 0,
                    HourlyRate = 55.0M,
                    //Rates = new ListingRates() { Hourly = 55.0M, FiveHours = 55.0M * 5, TenHours = 55.0M * 10 },
                    Name = "Social Media Marketing Strategies",
                    Description = "Master social media marketing strategies and engagement techniques for brand growth.",
                    //ListingImagePath = "assets/img/mentor/charlotte_anderson.jpg",
                    Locations = ListingLocationType.Webcam | ListingLocationType.TutorLocation,
                    //AboutYou = "I'm Charlotte, a marketing strategist with expertise in digital advertising and social media.",
                    //AboutLesson = "Lessons focus on content creation, engagement strategies, and data-driven marketing approaches.",
                }

            };

            context.Listings.AddRange(listings);
            context.SaveChanges();
        }
    }
}


public class ListingLessonCategorySeeder
{
    public static void Seed(AvanciraDbContext context, UserManager<User> userManager)
    {
        if (!context.ListingLessonCategories.Any())
        {
            var listingLessonCategories = new List<ListingLessonCategory>
            {
                new ListingLessonCategory
                {
                    ListingId = context.Listings.FirstOrDefault(c => EF.Functions.Like(c.Name, "Advanced Programming Lessons (C++)"))?.Id ?? 0,
                    LessonCategoryId = context.LessonCategories.FirstOrDefault(c => EF.Functions.Like(c.Name, "C++"))?.Id ?? 0,
                },
                new ListingLessonCategory
                {
                    ListingId = context.Listings.FirstOrDefault(c => EF.Functions.Like(c.Name, "AWS and DevOps Fundamentals"))?.Id ?? 0,
                    LessonCategoryId = context.LessonCategories.FirstOrDefault(c => EF.Functions.Like(c.Name, "AWS"))?.Id ?? 0,
                },
                new ListingLessonCategory
                {
                    ListingId = context.Listings.FirstOrDefault(c => EF.Functions.Like(c.Name, "Introduction to Machine Learning"))?.Id ?? 0,
                    LessonCategoryId = context.LessonCategories.FirstOrDefault(c => EF.Functions.Like(c.Name, "Machine Learning"))?.Id ?? 0,
                },
                new ListingLessonCategory
                {
                    ListingId = context.Listings.FirstOrDefault(c => EF.Functions.Like(c.Name, "Computer Architecture Tutoring"))?.Id ?? 0,
                    LessonCategoryId = context.LessonCategories.FirstOrDefault(c => EF.Functions.Like(c.Name, "Computer Science"))?.Id ?? 0,
                },
                new ListingLessonCategory
                {
                    ListingId = context.Listings.FirstOrDefault(c => EF.Functions.Like(c.Name, "Python for Data Science"))?.Id ?? 0,
                    LessonCategoryId = context.LessonCategories.FirstOrDefault(c => EF.Functions.Like(c.Name, "Python"))?.Id ?? 0,
                },
                new ListingLessonCategory
                {
                    ListingId = context.Listings.FirstOrDefault(c => EF.Functions.Like(c.Name, "Mathematics Tutoring for All Levels"))?.Id ?? 0,
                    LessonCategoryId = context.LessonCategories.FirstOrDefault(c => EF.Functions.Like(c.Name, "Maths"))?.Id ?? 0,
                },
                new ListingLessonCategory
                {
                    ListingId = context.Listings.FirstOrDefault(c => EF.Functions.Like(c.Name, "Java Programming Essentials"))?.Id ?? 0,
                    LessonCategoryId = context.LessonCategories.FirstOrDefault(c => EF.Functions.Like(c.Name, "Java"))?.Id ?? 0,
                },
                // Frontend Development - Angular
                new ListingLessonCategory
                {
                    ListingId = context.Listings.FirstOrDefault(c => EF.Functions.Like(c.Name, "Frontend Development with Angular"))?.Id ?? 0,
                    LessonCategoryId = context.LessonCategories.FirstOrDefault(c => EF.Functions.Like(c.Name, "Frontend Development"))?.Id ?? 0,
                },
                // Frontend Development - React
                new ListingLessonCategory
                {
                    ListingId = context.Listings.FirstOrDefault(c => EF.Functions.Like(c.Name, "Frontend Development with React"))?.Id ?? 0,
                    LessonCategoryId = context.LessonCategories.FirstOrDefault(c => EF.Functions.Like(c.Name, "Frontend Development"))?.Id ?? 0,
                },
                new ListingLessonCategory
                {
                    ListingId = context.Listings.FirstOrDefault(c => EF.Functions.Like(c.Name, "Backend Development with .NET"))?.Id ?? 0,
                    LessonCategoryId = context.LessonCategories.FirstOrDefault(c => EF.Functions.Like(c.Name, "Backend Development"))?.Id ?? 0,
                },
                new ListingLessonCategory
                {
                    ListingId = context.Listings.FirstOrDefault(c => EF.Functions.Like(c.Name, "Advanced Backend Development with .NET"))?.Id ?? 0,
                    LessonCategoryId = context.LessonCategories.FirstOrDefault(c => EF.Functions.Like(c.Name, "Backend Development"))?.Id ?? 0,
                },
                new ListingLessonCategory
                {
                    ListingId = context.Listings.FirstOrDefault(c => EF.Functions.Like(c.Name, "AWS Solutions Architect Masterclass"))?.Id ?? 0,
                    LessonCategoryId = context.LessonCategories.FirstOrDefault(c => EF.Functions.Like(c.Name, "Cloud Computing"))?.Id ?? 0,
                },
                new ListingLessonCategory
                {
                    ListingId = context.Listings.FirstOrDefault(c => EF.Functions.Like(c.Name, "Google Cloud Platform for Developers"))?.Id ?? 0,
                    LessonCategoryId = context.LessonCategories.FirstOrDefault(c => EF.Functions.Like(c.Name, "Cloud Computing"))?.Id ?? 0,
                },
                new ListingLessonCategory
                {
                    ListingId = context.Listings.FirstOrDefault(c => EF.Functions.Like(c.Name, "Full-Stack Web Development with MERN"))?.Id ?? 0,
                    LessonCategoryId = context.LessonCategories.FirstOrDefault(c => EF.Functions.Like(c.Name, "Web Development"))?.Id ?? 0,
                },
                new ListingLessonCategory
                {
                    ListingId = context.Listings.FirstOrDefault(c => EF.Functions.Like(c.Name, "Ethical Hacking with Kali Linux"))?.Id ?? 0,
                    LessonCategoryId = context.LessonCategories.FirstOrDefault(c => EF.Functions.Like(c.Name, "Cybersecurity"))?.Id ?? 0,
                },
                new ListingLessonCategory
                {
                    ListingId = context.Listings.FirstOrDefault(c => EF.Functions.Like(c.Name, "Mastering Photoshop & Illustrator for Designers"))?.Id ?? 0,
                    LessonCategoryId = context.LessonCategories.FirstOrDefault(c => EF.Functions.Like(c.Name, "Graphic Design"))?.Id ?? 0,
                },
                new ListingLessonCategory
                {
                    ListingId = context.Listings.FirstOrDefault(c => EF.Functions.Like(c.Name, "Financial Planning & Investment Strategies"))?.Id ?? 0,
                    LessonCategoryId = context.LessonCategories.FirstOrDefault(c => EF.Functions.Like(c.Name, "Finance"))?.Id ?? 0,
                },
                new ListingLessonCategory
                {
                    ListingId = context.Listings.FirstOrDefault(c => EF.Functions.Like(c.Name, "Corporate Finance & Accounting"))?.Id ?? 0,
                    LessonCategoryId = context.LessonCategories.FirstOrDefault(c => EF.Functions.Like(c.Name, "Finance"))?.Id ?? 0,
                },
                new ListingLessonCategory
                {
                    ListingId = context.Listings.FirstOrDefault(c => EF.Functions.Like(c.Name, "Digital Marketing & Social Media Strategy"))?.Id ?? 0,
                    LessonCategoryId = context.LessonCategories.FirstOrDefault(c => EF.Functions.Like(c.Name, "Marketing"))?.Id ?? 0,
                },
                new ListingLessonCategory
                {
                    ListingId = context.Listings.FirstOrDefault(c => EF.Functions.Like(c.Name, "Business Analytics with Excel & Power BI"))?.Id ?? 0,
                    LessonCategoryId = context.LessonCategories.FirstOrDefault(c => EF.Functions.Like(c.Name, "Business Analytics"))?.Id ?? 0,
                },
                new ListingLessonCategory
                {
                    ListingId = context.Listings.FirstOrDefault(c => EF.Functions.Like(c.Name, "Data Science & Business Forecasting"))?.Id ?? 0,
                    LessonCategoryId = context.LessonCategories.FirstOrDefault(c => EF.Functions.Like(c.Name, "Business Analytics"))?.Id ?? 0,
                },
                new ListingLessonCategory
                {
                    ListingId = context.Listings.FirstOrDefault(c => EF.Functions.Like(c.Name, "Leadership & Management Skills"))?.Id ?? 0,
                    LessonCategoryId = context.LessonCategories.FirstOrDefault(c => EF.Functions.Like(c.Name, "Management"))?.Id ?? 0,
                },
                new ListingLessonCategory
                {
                    ListingId = context.Listings.FirstOrDefault(c => EF.Functions.Like(c.Name, "Project Management & Agile Methodologies"))?.Id ?? 0,
                    LessonCategoryId = context.LessonCategories.FirstOrDefault(c => EF.Functions.Like(c.Name, "Management"))?.Id ?? 0,
                },
                new ListingLessonCategory
                {
                    ListingId = context.Listings.FirstOrDefault(c => EF.Functions.Like(c.Name, "Photography Masterclass: From Beginner to Pro"))?.Id ?? 0,
                    LessonCategoryId = context.LessonCategories.FirstOrDefault(c => EF.Functions.Like(c.Name, "Photography"))?.Id ?? 0,
                },
                new ListingLessonCategory
                {
                    ListingId = context.Listings.FirstOrDefault(c => EF.Functions.Like(c.Name, "Advanced Editing Techniques in Lightroom & Photoshop"))?.Id ?? 0,
                    LessonCategoryId = context.LessonCategories.FirstOrDefault(c => EF.Functions.Like(c.Name, "Photography"))?.Id ?? 0,
                },
                new ListingLessonCategory
                {
                    ListingId = context.Listings.FirstOrDefault(c => EF.Functions.Like(c.Name, "Creative Writing & Blogging Essentials"))?.Id ?? 0,
                    LessonCategoryId = context.LessonCategories.FirstOrDefault(c => EF.Functions.Like(c.Name, "Writing"))?.Id ?? 0,
                },
                new ListingLessonCategory
                {
                    ListingId = context.Listings.FirstOrDefault(c => EF.Functions.Like(c.Name, "Freelance Writing & Monetizing Your Content"))?.Id ?? 0,
                    LessonCategoryId = context.LessonCategories.FirstOrDefault(c => EF.Functions.Like(c.Name, "Writing"))?.Id ?? 0,
                },
                new ListingLessonCategory
                {
                    ListingId = context.Listings.FirstOrDefault(c => EF.Functions.Like(c.Name, "Graphic Design with Adobe Illustrator & Photoshop"))?.Id ?? 0,
                    LessonCategoryId = context.LessonCategories.FirstOrDefault(c => EF.Functions.Like(c.Name, "Graphic Design"))?.Id ?? 0,
                },
                new ListingLessonCategory
                {
                    ListingId = context.Listings.FirstOrDefault(c => EF.Functions.Like(c.Name, "Video Editing with Adobe Premiere Pro & After Effects"))?.Id ?? 0,
                    LessonCategoryId = context.LessonCategories.FirstOrDefault(c => EF.Functions.Like(c.Name, "Video Editing"))?.Id ?? 0,
                },
                new ListingLessonCategory
                {
                    ListingId = context.Listings.FirstOrDefault(c => EF.Functions.Like(c.Name, "YouTube Content Creation & Video Marketing"))?.Id ?? 0,
                    LessonCategoryId = context.LessonCategories.FirstOrDefault(c => EF.Functions.Like(c.Name, "Video Editing"))?.Id ?? 0,
                },
                new ListingLessonCategory
                {
                    ListingId = context.Listings.FirstOrDefault(c => EF.Functions.Like(c.Name, "Data Research and Analytics"))?.Id ?? 0,
                    LessonCategoryId = context.LessonCategories.FirstOrDefault(c => EF.Functions.Like(c.Name, "Research"))?.Id ?? 0,
                },
                new ListingLessonCategory
                {
                    ListingId = context.Listings.FirstOrDefault(c => EF.Functions.Like(c.Name, "Business Analytics and Data Science"))?.Id ?? 0,
                    LessonCategoryId = context.LessonCategories.FirstOrDefault(c => EF.Functions.Like(c.Name, "Business Analytics"))?.Id ?? 0,
                },
                new ListingLessonCategory
                {
                    ListingId = context.Listings.FirstOrDefault(c => EF.Functions.Like(c.Name, "Business Strategy and Market Research"))?.Id ?? 0,
                    LessonCategoryId = context.LessonCategories.FirstOrDefault(c => EF.Functions.Like(c.Name, "Business"))?.Id ?? 0,
                },
                new ListingLessonCategory
                {
                    ListingId = context.Listings.FirstOrDefault(c => EF.Functions.Like(c.Name, "Finance and Investment Strategies"))?.Id ?? 0,
                    LessonCategoryId = context.LessonCategories.FirstOrDefault(c => EF.Functions.Like(c.Name, "Finance"))?.Id ?? 0,
                },
                new ListingLessonCategory
                {
                    ListingId = context.Listings.FirstOrDefault(c => EF.Functions.Like(c.Name, "Deep Learning and AI Fundamentals"))?.Id ?? 0,
                    LessonCategoryId = context.LessonCategories.FirstOrDefault(c => EF.Functions.Like(c.Name, "Machine Learning"))?.Id ?? 0,
                },
                new ListingLessonCategory
                {
                    ListingId = context.Listings.FirstOrDefault(c => EF.Functions.Like(c.Name, "Ethical Hacking and Cybersecurity Basics"))?.Id ?? 0,
                    LessonCategoryId = context.LessonCategories.FirstOrDefault(c => EF.Functions.Like(c.Name, "Cybersecurity"))?.Id ?? 0,
                },
                new ListingLessonCategory
                {
                    ListingId = context.Listings.FirstOrDefault(c => EF.Functions.Like(c.Name, "Academic Writing and Research Skills"))?.Id ?? 0,
                    LessonCategoryId = context.LessonCategories.FirstOrDefault(c => EF.Functions.Like(c.Name, "Writing"))?.Id ?? 0,
                },
                new ListingLessonCategory
                {
                    ListingId = context.Listings.FirstOrDefault(c => EF.Functions.Like(c.Name, "Japanese and English Language Lessons"))?.Id ?? 0,
                    LessonCategoryId = context.LessonCategories.FirstOrDefault(c => EF.Functions.Like(c.Name, "Languages"))?.Id ?? 0,
                },
                new ListingLessonCategory
                {
                    ListingId = context.Listings.FirstOrDefault(c => EF.Functions.Like(c.Name, "Physics and Space Science Lessons"))?.Id ?? 0,
                    LessonCategoryId = context.LessonCategories.FirstOrDefault(c => EF.Functions.Like(c.Name, "Physics"))?.Id ?? 0,
                },
                new ListingLessonCategory
                {
                    ListingId = context.Listings.FirstOrDefault(c => EF.Functions.Like(c.Name, "Classical and Contemporary Music Lessons"))?.Id ?? 0,
                    LessonCategoryId = context.LessonCategories.FirstOrDefault(c => EF.Functions.Like(c.Name, "Music"))?.Id ?? 0,
                },
                new ListingLessonCategory
                {
                    ListingId = context.Listings.FirstOrDefault(c => EF.Functions.Like(c.Name, "Market Research and Business Analytics"))?.Id ?? 0,
                    LessonCategoryId = context.LessonCategories.FirstOrDefault(c => EF.Functions.Like(c.Name, "Market Research"))?.Id ?? 0,
                },
                new ListingLessonCategory
                {
                    ListingId = context.Listings.FirstOrDefault(c => EF.Functions.Like(c.Name, "Social Media Marketing Strategies"))?.Id ?? 0,
                    LessonCategoryId = context.LessonCategories.FirstOrDefault(c => EF.Functions.Like(c.Name, "Social Media"))?.Id ?? 0,
                },
            };

            context.ListingLessonCategories.AddRange(listingLessonCategories);
            context.SaveChanges();
        }
    }
}


public class LessonSeeder
{
    public static void Seed(AvanciraDbContext context)
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
    public static void Seed(AvanciraDbContext context)
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
    public static void Seed(AvanciraDbContext context)
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
    public static void Seed(AvanciraDbContext context)
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
            };

            context.Messages.AddRange(chats);
            context.SaveChanges();
        }
    }
}



