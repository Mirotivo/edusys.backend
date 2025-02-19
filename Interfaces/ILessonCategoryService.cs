public interface ILessonCategoryService
{
    // Create
    LessonCategoryDto CreateCategory(LessonCategory category);

    // Read
    List<LessonCategoryDto> GetLandingPageCategories();
    List<LessonCategoryDto> SearchCategories(string? query);

    // Update
    LessonCategoryDto UpdateCategory(int id, LessonCategory updatedCategory);

    // Delete
    bool DeleteCategory(int id);
}
