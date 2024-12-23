public interface ILessonCategoryService
{
    List<LessonCategory> GetDashboardCategories();
    List<LessonCategory> GetCategories();
    LessonCategory CreateCategory(LessonCategory category);
    LessonCategory UpdateCategory(int id, LessonCategory updatedCategory);
    bool DeleteCategory(int id);
}
