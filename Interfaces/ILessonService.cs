public interface ILessonService
{
    // Create
    Task<LessonDto> ProposeLessonAsync(LessonDto lessonDto, string userId);

    // Read
    Task<List<LessonDto>> GetLessonPropositionsAsync(string contactId, string userId, int listingId);
    Task<List<LessonDto>> GetLessonsAsync(string contactId, string userId, int listingId);

    // Update
    Task<bool> RespondToPropositionAsync(int lessonId, bool accept, string userId);

    // Delete
    Task<Lesson> CancelLessonAsync(int lessonId, string userId);
}
