using System.Collections.Generic;
using System.Threading.Tasks;

public interface ILessonCategoryService
{
    // Create
    LessonCategoryDto CreateCategory(LessonCategory category);

    // Read
    List<LessonCategoryDto> GetLandingPageCategories();
    Task<PagedResult<LessonCategoryDto>> SearchCategoriesAsync(string? query, int page, int pageSize);

    // Update
    LessonCategoryDto UpdateCategory(int id, LessonCategory updatedCategory);

    // Delete
    bool DeleteCategory(int id);
}

