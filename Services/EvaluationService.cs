using Microsoft.EntityFrameworkCore;

public class EvaluationService : IEvaluationService
{
    private readonly INotificationService _notificationService;
    private readonly AvanciraDbContext _dbContext;
    private readonly ILogger<EvaluationService> _logger;

    public EvaluationService(
        INotificationService notificationService,
        AvanciraDbContext dbContext,
        ILogger<EvaluationService> logger
    )
    {
        _notificationService = notificationService;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<IEnumerable<ReviewDto>> GetPendingReviews(string userId)
    {
        var lessons = await _dbContext.Lessons
            .Include(l => l.Listing)
            .Where(l => (l.Listing.UserId == userId || l.StudentId == userId) && l.Status == LessonStatus.Completed)
            .ToListAsync();
        var pendingReviews = lessons
            .SelectMany(l => new[]
            {
                new { ReviewerId = userId, RevieweeId = l.StudentId, Role = "Student", ReviewExists = _dbContext.Reviews.Any(r => r.ReviewerId == userId && r.RevieweeId == l.StudentId && r.Type == ReviewType.Review) },
                new { ReviewerId = userId, RevieweeId = l.Listing.UserId, Role = "Tutor", ReviewExists = _dbContext.Reviews.Any(r => r.ReviewerId == userId && r.RevieweeId == l.Listing.UserId && r.Type == ReviewType.Review) }
            })
            .Where(x => !x.ReviewExists) // Only include those without a review
            .Where(x => x.RevieweeId != userId) // Exclude cases where the user is reviewing themselves
            .Distinct()
            .Select(x =>
            {
                var reviewee = _dbContext.Users.FirstOrDefault(u => u.Id == x.RevieweeId);
                return new ReviewDto
                {
                    RevieweeId = x.RevieweeId,
                    Name = reviewee?.FirstName ?? reviewee?.Email,
                    Subject = $"Pending Review for {x.Role}",
                    Feedback = $"You have not reviewed this {x.Role.ToLower()} yet.",
                    Avatar = reviewee?.ProfileImagePath
                };
            })
            .ToList();
        return pendingReviews;
    }

    public async Task<IEnumerable<ReviewDto>> GetReceivedReviews(string userId)
    {
        return await _dbContext.Reviews
            .Where(r => r.Type == ReviewType.Review && r.RevieweeId == userId)
            .Include(r => r.Reviewer)
            .Select(r => new ReviewDto
            {
                RevieweeId = r.RevieweeId,
                Name = r.Reviewer.FirstName ?? r.Reviewer.Email,
                Subject = r.Title,
                Feedback = r.Comments,
                Avatar = r.Reviewer.ProfileImagePath
            })
            .ToListAsync();
    }

    public async Task<IEnumerable<ReviewDto>> GetSentReviews(string userId)
    {
        return await _dbContext.Reviews
            .Where(r => r.Type == ReviewType.Review && r.ReviewerId == userId)
            .Include(r => r.Reviewee)
            .Select(r => new ReviewDto
            {
                RevieweeId = r.RevieweeId,
                Name = r.Reviewee.FirstName ?? r.Reviewee.Email,
                Subject = r.Title,
                Feedback = r.Comments,
                Avatar = r.Reviewee.ProfileImagePath
            })
            .ToListAsync();
    }

    public async Task<IEnumerable<ReviewDto>> GetRecommendations(string userId)
    {
        return await _dbContext.Reviews
            .Where(r => r.Type == ReviewType.Recommendation && r.RevieweeId == userId)
            .Include(r => r.Reviewer)
            .Select(r => new ReviewDto
            {
                RevieweeId = r.RevieweeId,
                Name = r.Reviewer.FirstName ?? r.Reviewer.Email,
                Subject = r.Title,
                Feedback = r.Comments,
                Avatar = r.Reviewer.ProfileImagePath
            })
            .ToListAsync();
    }

    public async Task<bool> LeaveReview(ReviewDto reviewDto, string userId)
    {
        var lessonExists = await _dbContext.Lessons.AnyAsync(l =>
            (l.StudentId == reviewDto.RevieweeId && l.Listing.UserId == userId && l.Status == LessonStatus.Completed) ||
            (l.StudentId == userId && l.Listing.UserId == reviewDto.RevieweeId && l.Status == LessonStatus.Completed));

        if (!lessonExists)
        {
            return false;
        }

        var review = new Review
        {
            ReviewerId = userId,
            RevieweeId = reviewDto.RevieweeId,
            Type = ReviewType.Review,
            Title = reviewDto.Subject,
            Comments = reviewDto.Feedback,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Reviews.Add(review);
        await _dbContext.SaveChangesAsync();

        // Notify the reviewee
        var reviewer = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
        var reviewerName = $"{reviewer?.FirstName} {reviewer?.LastName}".Trim();
        await _notificationService.NotifyAsync(
            reviewDto.RevieweeId.ToString(),
            NotificationEvent.NewReviewReceived,
            $"You have received a new review from {reviewerName}.",
            new
            {
                ReviewId = review.Id,
                ReviewerId = userId,
                Title = reviewDto.Subject,
                Comments = reviewDto.Feedback,
                Timestamp = review.CreatedAt
            }
        );

        return true;
    }

    public async Task<bool> SubmitRecommendation(ReviewDto reviewDto, string userId)
    {
        var recommendation = new Review
        {
            ReviewerId = userId,
            RevieweeId = reviewDto.RevieweeId,
            Type = ReviewType.Recommendation,
            Title = reviewDto.Subject ?? string.Empty,
            Comments = reviewDto.Feedback ?? string.Empty,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Reviews.Add(recommendation);
        await _dbContext.SaveChangesAsync();

        // Notify the reviewee
        var reviewer = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
        var reviewerName = $"{reviewer?.FirstName} {reviewer?.LastName}".Trim();
        await _notificationService.NotifyAsync(
            reviewDto.RevieweeId.ToString(),
            NotificationEvent.NewReviewReceived,
            $"You have received a new recommendation from {reviewerName}.",
            new
            {
                RecommendationId = recommendation.Id,
                ReviewerId = userId,
                Title = reviewDto.Subject,
                Comments = reviewDto.Feedback,
                Timestamp = recommendation.CreatedAt
            }
        );

        return true;
    }
}
