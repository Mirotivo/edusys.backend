using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public class LessonCategoryService : ILessonCategoryService
{
    private readonly AvanciraDbContext _dbContext;
    private readonly ILogger<LessonCategoryService> _logger;

    public LessonCategoryService(
        AvanciraDbContext db,
        ILogger<LessonCategoryService> logger
    )
    {
        _dbContext = db;
        _logger = logger;
    }

    public LessonCategoryDto CreateCategory(LessonCategory category)
    {
        _dbContext.LessonCategories.Add(category);
        _dbContext.SaveChanges();
        return MapToLessonCategoryDto(category);
    }

    public List<LessonCategoryDto> GetLandingPageCategories()
    {
        return _dbContext.LessonCategories
            .OrderBy(_ => Guid.NewGuid())
            .Where(category => category.DisplayInLandingPage)
            .Take(12)
            .Select(category => MapToLessonCategoryDto(category, _dbContext.ListingLessonCategories.Include(l => l.Listing).Count(l => l.Listing.Active && l.Listing.IsVisible && l.LessonCategoryId == category.Id)))
            .ToList();
    }

    public async Task<PagedResult<LessonCategoryDto>> SearchCategoriesAsync(string? query, int page, int pageSize)
    {
        var queryable = _dbContext.LessonCategories
            .Where(l => EF.Functions.Like(l.Name, $"%{query}%"))
            .OrderBy(l => l.Name);

        // Get total count before pagination
        var totalResults = await queryable.CountAsync();

        // Apply pagination
        var categories = await queryable
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var results = categories.Select(category => MapToLessonCategoryDto(category)).ToList();

        return new PagedResult<LessonCategoryDto>(
            results: results,
            totalResults: totalResults,
            page: page,
            pageSize: pageSize
        );
    }


    public LessonCategoryDto UpdateCategory(int id, LessonCategory updatedCategory)
    {
        var category = _dbContext.LessonCategories.AsTracking().FirstOrDefault(l => l.Id == id);
        if (category == null)
            throw new ArgumentException("Can't update Lesson category.");

        category.Name = updatedCategory.Name;
        _dbContext.LessonCategories.Update(category);
        _dbContext.SaveChanges();
        return MapToLessonCategoryDto(category);
    }

    public bool DeleteCategory(int id)
    {
        var category = _dbContext.LessonCategories.AsTracking().FirstOrDefault(l => l.Id == id);
        if (category == null)
        {
            return false; // Or throw an exception
        }

        _dbContext.LessonCategories.Remove(category);
        _dbContext.SaveChanges();
        return true;
    }

    private LessonCategoryDto MapToLessonCategoryDto(LessonCategory category, int? courses = null)
    {
        return new LessonCategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Image = category.ImageUrl,
            Courses = courses ?? 0
        };
    }
}

