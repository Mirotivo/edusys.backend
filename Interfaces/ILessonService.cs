public interface ILessonService
{
    // Create
    Task<LessonDto> ProposeLessonAsync(LessonDto lessonDto, string userId);

    // Read
    Task<PagedResult<LessonDto>> GetLessonsAsync(string contactId, string userId, int listingId, int page, int pageSize);
    Task<PagedResult<LessonDto>> GetAllLessonsAsync(string userId, int page, int pageSize);

    // Update
    Task<bool> RespondToPropositionAsync(int lessonId, bool accept, string userId);
    Task ProcessPastBookedLessons();

    // Delete
    Task<Lesson> CancelLessonAsync(int lessonId, string userId);
}
