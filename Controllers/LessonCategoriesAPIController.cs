using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace avancira.Controllers;

[Route("api/lesson/categories")]
[ApiController]
public class LessonCategoriesAPIController : BaseController
{
    private readonly ILessonCategoryService _categoryService;
    private readonly ILogger<LessonCategoriesAPIController> _logger;

    public LessonCategoriesAPIController(ILessonCategoryService categoryService, ILogger<LessonCategoriesAPIController> logger)
    {
        _categoryService = categoryService;
        _logger = logger;
    }

    [HttpGet("dashboard")]
    public IActionResult Dashboard()
    {
        var categories = _categoryService.GetDashboardCategories();
        return JsonOk(categories);
    }

    [HttpGet()]
    public IActionResult GetCategories([FromQuery] string? query)
    {
        var categories = _categoryService.GetCategories(query);
        return JsonOk(categories);
    }

    [HttpPost]
    public IActionResult CreateCategory([FromBody] LessonCategory category)
    {
        if (category == null)
        {
            return JsonError();
        }

        var createdCategory = _categoryService.CreateCategory(category);
        return CreatedAtAction(nameof(GetCategories), new { id = createdCategory.Id }, createdCategory);
    }

    [HttpPut("{id}")]
    public IActionResult UpdateCategory(int id, [FromBody] LessonCategory updatedCategory)
    {
        var category = _categoryService.UpdateCategory(id, updatedCategory);
        if (category == null)
        {
            return NotFound();
        }

        return JsonOk(category);
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteCategory(int id)
    {
        var result = _categoryService.DeleteCategory(id);
        if (!result)
        {
            return NotFound();
        }

        return NoContent();
    }
}

