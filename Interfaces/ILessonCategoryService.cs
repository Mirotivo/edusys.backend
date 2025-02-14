public interface ILessonCategoryService
{
    List<LessonCategoryDto> GetLandingPageCategories();
    List<LessonCategoryDto> GetCategories(string? query);
    LessonCategoryDto CreateCategory(LessonCategory category);
    LessonCategoryDto UpdateCategory(int id, LessonCategory updatedCategory);
    bool DeleteCategory(int id);
}
