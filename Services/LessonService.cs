using Microsoft.EntityFrameworkCore;

public class LessonService : ILessonService
{
    private readonly skillseekDbContext _dbContext;
    private readonly INotificationService _notificationService;
    private readonly IPaymentService _paymentService;

    public LessonService(
        skillseekDbContext dbContext,
        INotificationService notificationService,
        IPaymentService paymentService
    )
    {
        _dbContext = dbContext;
        _notificationService = notificationService;
        _paymentService = paymentService;
    }

    public async Task<LessonDto> ProposeLesson(LessonDto lessonDto, int userId)
    {
        if (lessonDto.StudentId == null || lessonDto.StudentId == 0)
        {
            lessonDto.StudentId = userId;
        }

        var lesson = new Lesson
        {
            Date = lessonDto.Date,
            Duration = lessonDto.Duration,
            Price = lessonDto.Price,
            StudentId = lessonDto.StudentId.Value,
            ListingId = lessonDto.ListingId,
            IsStudentInitiated = lessonDto.StudentId == userId,
            Status = LessonStatus.Proposed
        };

        await _dbContext.Lessons.AddAsync(lesson);
        await _dbContext.SaveChangesAsync();

        // Notify the tutor
        var student = await _dbContext.Users
            .Where(u => u.Id == userId)
            .Select(u => new { u.FirstName, u.LastName })
            .FirstOrDefaultAsync();

        var tutorId = await _dbContext.Listings
            .Where(l => l.Id == lessonDto.ListingId)
            .Select(l => l.UserId)
            .FirstOrDefaultAsync();

        if (tutorId != 0)
        {
            var studentName = $"{student?.FirstName} {student?.LastName}".Trim();

            await _notificationService.NotifyAsync(
                tutorId.ToString(),
                NotificationEvent.ChatRequest,
                $"{studentName} has proposed a new lesson.",
                new
                {
                    LessonId = lesson.Id,
                    Date = lesson.Date,
                    Duration = lesson.Duration,
                    Price = lesson.Price
                }
            );
        }

        return new LessonDto
        {
            Id = lesson.Id,
            Date = lesson.Date,
            Duration = lesson.Duration,
            Price = lesson.Price,
            Status = lesson.Status
        };
    }

    public async Task<bool> RespondToProposition(int lessonId, bool accept, int userId)
    {
        var lesson = await _dbContext.Lessons
            .Include(l=> l.Student)
            .Include(l => l.Listing)
            .ThenInclude(l => l.User)
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
                var transaction = await _paymentService.ProcessTransactionAsync(
                    stripeCustomerId: lesson.Student.StripeCustomerId,
                    senderId: lesson.StudentId,
                    recipientId: lesson.Listing.UserId,
                    amount: lesson.Price,
                    paymentType: PaymentType.Lesson
                );

                lesson.Status = LessonStatus.Booked;
                lesson.TransactionId = transaction.Id;
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

        if (studentId != 0)
        {
            var tutorName = $"{lesson.Listing.User.FirstName} {lesson.Listing.User.LastName}".Trim();
            var lessonTitle = lesson.Listing.Title ?? "the lesson";

            var statusMessage = accept
                ? $"Your proposition for {lessonTitle} has been accepted by {tutorName}."
                : $"Your proposition for {lessonTitle} has been declined by {tutorName}.";

            await _notificationService.NotifyAsync(
                studentId.ToString(),
                NotificationEvent.SystemAlert,
                statusMessage,
                new
                {
                    LessonId = lesson.Id,
                    Status = lesson.Status
                }
            );
        }

        return true;
    }

    public async Task<List<LessonDto>> GetPropositionsAsync(int contactId, int userId)
    {
        return await _dbContext.Lessons
            .Where(p => p.Status == LessonStatus.Proposed)
            .Where(p => p.IsStudentInitiated && userId == p.Listing.UserId)
            .Where(p => p.StudentId == contactId)
            .OrderByDescending(p => p.Date)
            .Select(p => new LessonDto
            {
                Id = p.Id,
                Date = p.Date,
                Duration = p.Duration,
                Price = p.Price,
                Status = p.Status
            })
            .ToListAsync();
    }

    public async Task<List<LessonDto>> GetLessonsAsync(int contactId, int userId)
    {
        return await _dbContext.Lessons
            .Where(p => (p.Status == LessonStatus.Booked || p.Status == LessonStatus.Completed || p.Status == LessonStatus.Canceled))
            .Where(p => p.StudentId == contactId)
            .OrderByDescending(p => p.Date)
            .Select(p => new LessonDto
            {
                Id = p.Id,
                Topic = "Lesson",
                Date = p.Date,
                Duration = p.Duration,
                Status = p.Status
            })
            .ToListAsync();
    }

    public async Task<Lesson> CancelLessonAsync(int lessonId, int userId)
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
        decimal refundAmount = lesson.Price;
        var lessonStartTime = lesson.Date;
        var currentTime = DateTime.UtcNow;

        decimal retainedAmount = 0;
        if (!isTutor && lessonStartTime.Subtract(currentTime).TotalHours <= 24)
        {
            // If the student cancels less than 1 day before the lesson, retain tutor compensation
            var tutorPercentage = lesson.Listing.User.TutorRefundRetention / 100m;
            retainedAmount = refundAmount * tutorPercentage;
            refundAmount -= retainedAmount; // Refund the remaining amount

            Console.WriteLine($"Partial refund applied. Retained: {retainedAmount:C}, Refunded: {refundAmount:C}.");
        }
        else
        {
            // Full refund if canceled more than 1 day before
            Console.WriteLine($"Full refund of {refundAmount:C} applied.");
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
                var transaction = await _paymentService.ProcessTransactionAsync(
                    stripeCustomerId: lesson.Student.StripeCustomerId,
                    senderId: lesson.StudentId,
                    recipientId: lesson.Listing.UserId,
                    amount: lesson.Price,
                    paymentType: PaymentType.Lesson
                );

                lesson.TransactionId = transaction.Id;
                await _dbContext.SaveChangesAsync();

                
                // Restore original price for lesson
                lesson.Price = originalPrice;

                Console.WriteLine($"Retained amount of {retainedAmount:C} paid to tutor.");
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
            Console.WriteLine($"Full refund of {lesson.Price:C} processed for student.");
        }
        else
        {
            Console.WriteLine($"Partial refund of {refundAmount:C} processed. Tutor retained: {lesson.Price - refundAmount:C}.");
        }

        return lesson;
    }
}
