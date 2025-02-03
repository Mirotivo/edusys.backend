public interface ILessonService
{
    Task<LessonDto> ProposeLesson(LessonDto lessonDto, string userId);
    Task<bool> RespondToProposition(int lessonId, bool accept, string userId);
    Task<List<LessonDto>> GetPropositionsAsync(string contactId, string userId, int listingId);
    Task<List<LessonDto>> GetLessonsAsync(string contactId, string userId, int listingId);
    Task<Lesson> CancelLessonAsync(int lessonId, string userId);
}
