using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;


public class RoleSeeder
{
    public static void Seed(AvanciraDbContext dbContext, UserManager<User> userManager)
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
    public static void Seed(AvanciraDbContext context, UserManager<User> userManager)
    {
        if (!context.Users.Any())
        {
            var users = new List<User>
            {
                new User
                {
                    FirstName = "Amr",
                    LastName = "Mostafa",
                    UserName = "Amr.Mostafa@live.com",
                    Email = "Amr.Mostafa@live.com",
                    Address = "76 Bancroft Ave, Roseville NSW 2069, Australia",
                    ProfileImagePath = $"https://robohash.org/{Guid.NewGuid()}?size=200x200&set=set1",
                },
                new User
                {
                    FirstName = "Student",
                    LastName = "",
                    UserName = "Student@gmail.com",
                    Email = "Student@gmail.com",
                    Address = "22 Bronte Road, Bondi Junction NSW 2022, Australia",
                    ProfileImagePath = $"https://robohash.org/{Guid.NewGuid()}?size=200x200&set=set1",
                },
                new User
                {
                    FirstName = "Tutor",
                    LastName = "",
                    UserName = "Tutor@gmail.com",
                    Email = "Tutor@gmail.com",
                    Address = "101 Grafton Street, Bondi Junction NSW 2022, Australia",
                    ProfileImagePath = $"https://robohash.org/{Guid.NewGuid()}?size=200x200&set=set1",
                },
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
                new LessonCategory { Name = "Maths" },
                new LessonCategory { Name = "Physics" },
                new LessonCategory { Name = "Chemistry" },
                new LessonCategory { Name = "Biology" },
                new LessonCategory { Name = "Science" },
                new LessonCategory { Name = "GAMSAT" },
                new LessonCategory { Name = "IELTS" },
                new LessonCategory { Name = "UCAT" },
                new LessonCategory { Name = "ESL" },

                // Languages
                new LessonCategory { Name = "English" },
                new LessonCategory { Name = "Spanish" },
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
                new LessonCategory { Name = "Piano" },
                new LessonCategory { Name = "Guitar" },
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
                new LessonCategory { Name = "Swimming" },
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
                new LessonCategory { Name = "C++" },
                new LessonCategory { Name = "Python" },
                new LessonCategory { Name = "Java" },
                new LessonCategory { Name = "Crypto Currency" },
                new LessonCategory { Name = "Blockchain" },
                new LessonCategory { Name = "Data Science" },
                new LessonCategory { Name = "Machine Learning" },
                new LessonCategory { Name = "Frontend Development" },
                new LessonCategory { Name = "Backend Development" },
                new LessonCategory { Name = "AWS" },
                new LessonCategory { Name = "Computer Science" },

                // Hobbies and Leisure
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
                    LessonCategoryId = context.LessonCategories.FirstOrDefault(c => EF.Functions.Like(c.Name, "C++"))?.Id ?? 0,
                    Rates = new ListingRates() { Hourly = 50.0M, FiveHours = 50.0M * 5, TenHours = 50.0M * 10 },
                    Title = "Advanced Programming Lessons (C++)",
                    Description = "Master programming concepts with hands-on lessons in C++ for beginners to advanced levels.",
                    ListingImagePath = $"https://robohash.org/{Guid.NewGuid()}?size=200x200&set=set1",
                    Locations = LocationType.Webcam | LocationType.StudentLocation,
                    AboutYou = "My name is Amr, and I’m a software engineer with years of experience in developing complex systems.",
                    AboutLesson = "Lessons focus on teaching C++ programming concepts with practical examples and projects.",
                },
                new Listing
                {
                    UserId = context.Users.FirstOrDefault(c => EF.Functions.Like(c.Email, "Amr.Mostafa@live.com"))?.Id ?? string.Empty,
                    LessonCategoryId = context.LessonCategories.FirstOrDefault(c => EF.Functions.Like(c.Name, "AWS"))?.Id ?? 0,
                    Rates = new ListingRates() { Hourly = 60.0M, FiveHours = 60.0M * 5, TenHours = 60.0M * 10 },
                    Title = "AWS and DevOps Fundamentals",
                    Description = "Learn AWS services (EC2, S3, RDS) and DevOps pipelines with tools like Jenkins and Docker.",
                    ListingImagePath = $"https://robohash.org/{Guid.NewGuid()}?size=200x200&set=set2",
                    Locations = LocationType.Webcam | LocationType.TutorLocation,
                    AboutYou = "My name is Amr, and I have extensive experience in cloud infrastructure and DevOps engineering.",
                    AboutLesson = "Lessons include setting up AWS services, CI/CD pipelines, and hands-on labs for deployment.",
                },
                new Listing
                {
                    UserId = context.Users.FirstOrDefault(c => EF.Functions.Like(c.Email, "Amr.Mostafa@live.com"))?.Id ?? string.Empty,
                    LessonCategoryId = context.LessonCategories.FirstOrDefault(c => EF.Functions.Like(c.Name, "Machine Learning"))?.Id ?? 0,
                    Rates = new ListingRates() { Hourly = 70.0M, FiveHours = 70.0M * 5, TenHours = 70.0M * 10 },
                    Title = "Introduction to Machine Learning",
                    Description = "Build foundational skills in machine learning, focusing on neural networks and deep learning with PyTorch.",
                    ListingImagePath = $"https://robohash.org/{Guid.NewGuid()}?size=200x200&set=set3",
                    Locations = LocationType.Webcam | LocationType.StudentLocation,
                    AboutYou = "I’m Amr, an experienced software engineer specializing in machine learning and AI technologies.",
                    AboutLesson = "Lessons cover the basics of machine learning, algorithm development, and practical implementations.",
                },
                new Listing
                {
                    UserId = context.Users.FirstOrDefault(c => EF.Functions.Like(c.Email, "Amr.Mostafa@live.com"))?.Id ?? string.Empty,
                    LessonCategoryId = context.LessonCategories.FirstOrDefault(c => EF.Functions.Like(c.Name, "Computer Science"))?.Id ?? 0,
                    Rates = new ListingRates() { Hourly = 55.0M, FiveHours = 55.0M * 5, TenHours = 55.0M * 10 },
                    Title = "Computer Architecture Tutoring",
                    Description = "Specialized lessons in computer architecture and FPGA design for students and professionals.",
                    ListingImagePath = $"https://robohash.org/{Guid.NewGuid()}?size=200x200&set=set4",
                    Locations = LocationType.Webcam | LocationType.StudentLocation | LocationType.TutorLocation,
                    AboutYou = "I’m Amr, a computer engineer with experience in hardware design and low-level programming.",
                    AboutLesson = "Lessons focus on understanding computer architecture, FPGA programming, and their applications.",
                },
                new Listing
                {
                    UserId = context.Users.FirstOrDefault(c => EF.Functions.Like(c.Email, "Amr.Mostafa@live.com"))?.Id ?? string.Empty,
                    LessonCategoryId = context.LessonCategories.FirstOrDefault(c => EF.Functions.Like(c.Name, "Python"))?.Id ?? 0,
                    Rates = new ListingRates() { Hourly = 45.0M, FiveHours = 45.0M * 5, TenHours = 45.0M * 10 },
                    Title = "Python for Data Science",
                    Description = "Learn Python programming for data analysis, visualization, and machine learning.",
                    ListingImagePath = $"https://robohash.org/{Guid.NewGuid()}?size=200x200&set=set5",
                    Locations = LocationType.Webcam | LocationType.TutorLocation,
                    AboutYou = "My name is Amr, and I’m a software engineer with expertise in Python for data science and AI.",
                    AboutLesson = "Lessons include Python basics, libraries like NumPy and Pandas, and practical data science projects.",
                },
            };

            context.Listings.AddRange(listings);
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


