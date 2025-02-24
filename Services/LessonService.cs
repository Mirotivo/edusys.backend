using System.Text.Json;
using Backend.Database.Models;
using Backend.Services.Events;
using Backend.Services.Notifications.Messages;
using Microsoft.EntityFrameworkCore;

public class LessonService : ILessonService
{
    const decimal platformFeeRate = 0.1m;
    private readonly AvanciraDbContext _dbContext;
    private readonly INotificationService _notificationService;
    private readonly IChatService _chatService;
    private readonly IPaymentService _paymentService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<LessonService> _logger;

    public LessonService(
        AvanciraDbContext dbContext,
        INotificationService notificationService,
        IChatService chatService,
        IPaymentService paymentService,
        IJwtTokenService jwtTokenService,
        ILogger<LessonService> logger
    )
    {
        _dbContext = dbContext;
        _notificationService = notificationService;
        _chatService = chatService;
        _paymentService = paymentService;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
    }

    public async Task<LessonDto> ProposeLessonAsync(LessonDto lessonDto, string userId)
    {
        if (lessonDto.StudentId == null || string.IsNullOrEmpty(lessonDto.StudentId))
        {
            lessonDto.StudentId = userId;
        }

        // Get tutor ID associated with the listing
        var tutorId = await _dbContext.Listings
            .Where(l => l.Id == lessonDto.ListingId)
            .Select(l => l.UserId)
            .FirstOrDefaultAsync();

        // Ensure chat exists before proposing the lesson
        var chat = _chatService.GetOrCreateChat(lessonDto.StudentId, tutorId ?? string.Empty, lessonDto.ListingId);

        var lesson = new Lesson
        {
            Date = DateTime.SpecifyKind(lessonDto.Date, DateTimeKind.Utc),
            Duration = lessonDto.Duration,
            Price = lessonDto.Price,
            StudentId = lessonDto.StudentId,
            ListingId = lessonDto.ListingId,
            IsStudentInitiated = lessonDto.StudentId == userId,
            Status = LessonStatus.Proposed
        };

        await _dbContext.Lessons.AddAsync(lesson);
        await _dbContext.SaveChangesAsync();

        // Notify the tutor
        var student = await _dbContext.Users
            .Where(u => u.Id == userId)
            .Select(u => new { u.FullName })
            .FirstOrDefaultAsync();


        if (!string.IsNullOrEmpty(tutorId))
        {
            var studentName = $"{student?.FullName}".Trim();

            var eventData = new BookingRequestedEvent
            {
                TutorId = tutorId,
                StudentId = lessonDto.StudentId,
                LessonId = lesson.Id,
                Date = lesson.Date,
                Duration = lesson.Duration,
                Price = lesson.Price,
                StudentName = studentName
            };
            await _notificationService.NotifyAsync(NotificationEvent.BookingRequested, eventData);
        }

        return MapToLessonDto(lesson, userId);
    }

    public async Task<List<LessonDto>> GetLessonPropositionsAsync(string contactId, string userId, int listingId)
    {
        var lessons = await _dbContext.Lessons
            .Include(p => p.Student)
            .Include(p => p.Listing).ThenInclude(p => p.User)
            .Where(p => p.Status == LessonStatus.Proposed)
            .Where(p => p.ListingId == listingId)
            .Where(p => (p.StudentId == contactId && p.Listing.UserId == userId) || (p.StudentId == userId && p.Listing.UserId == contactId))
            .OrderByDescending(p => p.Date)
            .ToListAsync();

        return lessons.Select(lesson => MapToLessonDto(lesson, userId)).ToList();
    }

    public async Task<List<LessonDto>> GetLessonsAsync(string contactId, string userId, int listingId)
    {
        var lessons = await _dbContext.Lessons
            .Include(p => p.Student)
            .Include(p => p.Listing).ThenInclude(p => p.User)
            .Where(p => (p.Status == LessonStatus.Booked || p.Status == LessonStatus.Completed || p.Status == LessonStatus.Canceled))
            .Where(p => p.ListingId == listingId)
            .Where(p => (p.StudentId == contactId && p.Listing.UserId == userId) || (p.StudentId == userId && p.Listing.UserId == contactId))
            .OrderByDescending(p => p.Date)
            .ToListAsync();

        return lessons.Select(lesson => MapToLessonDto(lesson, userId)).ToList();
    }

    public async Task<List<LessonDto>> GetAllLessonPropositionsAsync(string userId)
    {
        var propositions = await _dbContext.Lessons
            .Include(p => p.Student)
            .Include(p => p.Listing).ThenInclude(p => p.User)
            .Where(p => p.Status == LessonStatus.Proposed)
            .Where(p => p.StudentId == userId || p.Listing.UserId == userId)
            .OrderByDescending(p => p.Date)
            .ToListAsync();

        return propositions.Select(lesson => MapToLessonDto(lesson, userId)).ToList();
    }

    public async Task<List<LessonDto>> GetAllLessonsAsync(string userId)
    {
        var lessons = await _dbContext.Lessons
            .Include(p => p.Student)
            .Include(p => p.Listing).ThenInclude(p => p.User)
            .Where(p => p.Status == LessonStatus.Booked ||
                        p.Status == LessonStatus.Completed ||
                        p.Status == LessonStatus.Canceled)
            .Where(p => p.StudentId == userId || p.Listing.UserId == userId)
            .OrderByDescending(p => p.Date)
            .ToListAsync();

        return lessons.Select(lesson => MapToLessonDto(lesson, userId)).ToList();
    }

    public async Task<bool> RespondToPropositionAsync(int lessonId, bool accept, string userId)
    {
        var lesson = await _dbContext.Lessons
            .Include(l => l.Student)
            .Include(l => l.Listing).ThenInclude(l => l.User)
            .FirstOrDefaultAsync(l => l.Id == lessonId);

        if (lesson == null)
        {
            return false;
        }

        if (accept)
        {
            try
            {
                // Process payment using the shared method
                var transaction = await _paymentService.ExecuteTransactionAsync(
                    stripeCustomerId: lesson.Student?.StripeCustomerId ?? string.Empty,
                    senderId: lesson.StudentId,
                    recipientId: lesson.Listing?.UserId,
                    amount: lesson.Price,
                    paymentType: PaymentType.Lesson
                );

                lesson.Status = LessonStatus.Booked;
                lesson.TransactionId = transaction.Id;


                // Generate meeting token and URL
                var username = lesson.Student?.FullName ?? "Student";
                var roomName = lesson.Listing?.Title.Replace(" ", "_") ?? "Lesson";
                var meeting = _jwtTokenService.GetMeeting(username, roomName);
                lesson.MeetingToken = meeting.Token;
                lesson.MeetingDomain = meeting.Domain;
                lesson.MeetingUrl = meeting.ServerUrl;
                lesson.MeetingRoomName = meeting.RoomName;
                lesson.MeetingRoomUrl = meeting.MeetingUrl;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to process payment: " + ex.Message);
            }
        }
        else
        {
            lesson.Status = LessonStatus.Canceled;
        }

        await _dbContext.SaveChangesAsync();

        // Notify the student
        var studentId = lesson.StudentId;

        if (!string.IsNullOrEmpty(studentId))
        {
            var tutorName = $"{lesson.Listing.User.FullName}".Trim();
            var lessonTitle = lesson.Listing.Title ?? "the lesson";
            var eventData = new PropositionRespondedEvent
            {
                StudentId = studentId,
                LessonId = lesson.Id,
                Status = lesson.Status,
                TutorName = tutorName,
                LessonTitle = lessonTitle,
                Date = lesson.Date,
                Duration = lesson.Duration,
                Price = lesson.Price,
                MeetingUrl = lesson.MeetingRoomUrl
            };

            await _notificationService.NotifyAsync(NotificationEvent.PropositionResponded, eventData);
        }

        return true;
    }

    public async Task<Lesson> CancelLessonAsync(int lessonId, string userId)
    {
        // Include the Listing for tutor and the Transaction for PaymentId
        var lesson = await _dbContext.Lessons
            .Include(l => l.Student)
            .Include(l => l.Listing)       // Include the Listing for tutor information
            .ThenInclude(l => l.User)
            .Include(l => l.Transaction)   // Include the linked Transaction
            .FirstOrDefaultAsync(l => l.Id == lessonId);

        if (lesson == null)
        {
            throw new KeyNotFoundException("Lesson not found.");
        }

        var isTutor = lesson.Listing.UserId == userId;

        if (!isTutor && lesson.StudentId != userId)
        {
            throw new UnauthorizedAccessException("You are not authorized to cancel this lesson.");
        }

        if (lesson.Status != LessonStatus.Booked)
        {
            throw new InvalidOperationException("Only booked lessons can be canceled.");
        }

        // Calculate refund amount
        var refundAmount = lesson.Price;
        var lessonStartTime = lesson.Date;
        var currentTime = DateTime.UtcNow;

        decimal retainedAmount = 0;
        if (!isTutor && lessonStartTime.Subtract(currentTime).TotalHours <= 24)
        {
            // If the student cancels less than 1 day before the lesson, retain tutor compensation
            var tutorPercentage = lesson.Listing.User.TutorRefundRetention / 100m;
            retainedAmount = refundAmount * tutorPercentage;
            refundAmount -= retainedAmount; // Refund the remaining amount

            _logger.LogInformation($"Partial refund applied. Retained: {retainedAmount:C}, Refunded: {refundAmount:C}.");
        }
        else
        {
            // Full refund if canceled more than 1 day before
            _logger.LogInformation($"Full refund of {refundAmount:C} applied.");
        }

        // Perform refund
        await _paymentService.RefundPaymentAsync(lesson.TransactionId ?? -1, lesson.Price, 0); // Fully Refund
        // await _paymentService.RefundPaymentAsync(lesson.TransactionId ?? -1, refundAmount, retainedAmount);

        // Check if there is a retained amount to pay the tutor
        if (retainedAmount > 0)
        {
            try
            {
                // Temporarily set lesson price to retainedAmount for payment
                var originalPrice = lesson.Price;
                lesson.Price = retainedAmount;

                // Pay the retained amount to the tutor
                var transaction = await _paymentService.ExecuteTransactionAsync(
                    stripeCustomerId: lesson.Student?.StripeCustomerId ?? string.Empty,
                    senderId: lesson.StudentId,
                    recipientId: lesson.Listing.UserId,
                    amount: lesson.Price,
                    paymentType: PaymentType.Lesson
                );

                lesson.TransactionId = transaction.Id;
                await _dbContext.SaveChangesAsync();


                // Restore original price for lesson
                lesson.Price = originalPrice;

                _logger.LogInformation($"Retained amount of {retainedAmount:C} paid to tutor.");
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to process retained amount payment: " + ex.Message);
            }
        }

        // Update lesson status
        lesson.Status = LessonStatus.Canceled;
        lesson.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        // Log or notify about the refund
        if (isTutor)
        {
            _logger.LogInformation($"Full refund of {lesson.Price:C} processed for student.");
        }
        else
        {
            _logger.LogInformation($"Partial refund of {refundAmount:C} processed. Tutor retained: {lesson.Price - refundAmount:C}.");
        }

        return lesson;
    }

    private LessonDto MapToLessonDto(Lesson lesson, string userId)
    {
        return new LessonDto
        {
            Id = lesson.Id,
            Date = lesson.Date,
            Duration = lesson.Duration,
            Price = lesson.Price,
            StudentId = lesson.StudentId,
            StudentName = lesson.Student?.FullName ?? "Unkown Student",
            TutorName = lesson.Listing?.User?.FullName ?? "Unknown Tutor",
            RecipientName = lesson.StudentId == userId
                    ? lesson.Listing?.User?.FullName ?? "Unknown Tutor"
                    : lesson.Student?.FullName ?? "Unknown Student",
            ListingId = lesson.ListingId,
            Status = lesson.Status,
            Topic = lesson.Listing?.Title ?? "Lesson",
            MeetingToken = lesson.MeetingToken,
            MeetingDomain = lesson.MeetingDomain,
            MeetingRoomName = lesson.MeetingRoomName,
            MeetingUrl = lesson.MeetingUrl,
            MeetingRoomUrl = lesson.MeetingRoomUrl
        };
    }
}
