public interface IEvaluationService
{
    Task<IEnumerable<ReviewDto>> GetPendingReviews(string userId);
    Task<IEnumerable<ReviewDto>> GetReceivedReviews(string userId);
    Task<IEnumerable<ReviewDto>> GetSentReviews(string userId);
    Task<IEnumerable<ReviewDto>> GetRecommendations(string userId);
    Task<bool> LeaveReview(ReviewDto reviewDto, string userId);
    Task<bool> SubmitRecommendation(ReviewDto reviewDto, string userId);
}
