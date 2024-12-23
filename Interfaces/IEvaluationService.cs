public interface IEvaluationService
{
    Task<IEnumerable<ReviewDto>> GetPendingReviews(int userId);
    Task<IEnumerable<ReviewDto>> GetReceivedReviews(int userId);
    Task<IEnumerable<ReviewDto>> GetSentReviews(int userId);
    Task<IEnumerable<ReviewDto>> GetRecommendations(int userId);
    Task<bool> LeaveReview(ReviewDto reviewDto, int userId);
    Task<bool> SubmitRecommendation(ReviewDto reviewDto, int userId);
}
