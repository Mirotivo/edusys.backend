using System.Diagnostics;
using System.Drawing.Printing;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Stripe;

namespace avancira.Controllers;

[Route("api/lessons")]
[ApiController]
public class LessonsAPIController : BaseController
{
    private readonly ILessonService _lessonService;
    private readonly ILogger<LessonsAPIController> _logger;

    public LessonsAPIController(
        ILessonService lessonService,
        ILogger<LessonsAPIController> logger
    )
    {
        _lessonService = lessonService;
        _logger = logger;
    }

    // Create
    [Authorize]
    [HttpPost("proposeLesson")]
    public async Task<IActionResult> ProposeLessonAsync([FromBody] LessonDto lessonDto)
    {
        var userId = GetUserId();
        var result = await _lessonService.ProposeLessonAsync(lessonDto, userId);
        return JsonOk(new { Message = "Lesson proposed successfully.", Lesson = result });
    }


    // Read
    [Authorize]
    [HttpGet("{contactId}/{listingId}")]
    public async Task<IActionResult> GetLessonsAsync(string contactId, int listingId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var userId = GetUserId();

        var lessons = await _lessonService.GetLessonsAsync(contactId, userId, listingId, page, pageSize);

        return JsonOk(new { Lessons = lessons });
    }

    [Authorize]
    [HttpGet()]
    public async Task<IActionResult> GetAllLessonsAsync([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var userId = GetUserId();

        var lessons = await _lessonService.GetAllLessonsAsync(userId, page, pageSize);

        return JsonOk(new { Lessons = lessons });
    }

    // Update
    [Authorize]
    [HttpPut("respondToProposition/{lessonId}")]
    public async Task<IActionResult> RespondToPropositionAsync(int lessonId, [FromBody] bool accept)
    {
        var userId = GetUserId();

        if (!await _lessonService.RespondToPropositionAsync(lessonId, accept, userId))
        {
            return NotFound(new { Message = "Lesson not found." });
        }

        return JsonOk(new { Message = accept ? "Proposition accepted." : "Proposition refused." });
    }

    // Delete
    [Authorize]
    [HttpDelete("{lessonId}/cancel")]
    public async Task<IActionResult> CancelLessonAsync(int lessonId)
    {
        try
        {
            var userId = GetUserId(); // Extract user ID from the token
            var canceledLesson = await _lessonService.CancelLessonAsync(lessonId, userId);

            return JsonOk(new
            {
                Message = "Lesson canceled successfully.",
                Lesson = new
                {
                    canceledLesson.Id,
                    canceledLesson.Status,
                    canceledLesson.Date,
                    canceledLesson.Duration
                }
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { Message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return JsonError(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "An error occurred while canceling the lesson.", Details = ex.Message });
        }
    }
}

