using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using skillseek.Models;
using Stripe;

namespace skillseek.Controllers;

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

    [HttpPost("proposeLesson")]
    public async Task<IActionResult> ProposeLessonAsync([FromBody] LessonDto lessonDto)
    {
        var userId = GetUserId();
        var result = await _lessonService.ProposeLesson(lessonDto, userId);
        return JsonOk(new { Message = "Lesson proposed successfully.", Lesson = result });
    }

    [HttpPost("respondToProposition/{lessonId}")]
    public async Task<IActionResult> RespondToPropositionAsync(int lessonId, [FromBody] bool accept)
    {
        var userId = GetUserId();

        if (!await _lessonService.RespondToProposition(lessonId, accept, userId))
        {
            return NotFound(new { Message = "Lesson not found." });
        }

        return JsonOk(new { Message = accept ? "Proposition accepted." : "Proposition refused." });
    }

    [HttpGet("{contactId}")]
    public async Task<IActionResult> GetLessonsAsync(int contactId)
    {
        var userId = GetUserId();

        // Call the separate service methods
        var propositionsTask = _lessonService.GetPropositionsAsync(contactId, userId);
        var lessonsTask = _lessonService.GetLessonsAsync(contactId, userId);

        // Await both tasks in parallel for efficiency
        await Task.WhenAll(propositionsTask, lessonsTask);

        var propositions = await propositionsTask;
        var lessons = await lessonsTask;

        // Return a combined result
        return JsonOk(new { Propositions = propositions, Lessons = lessons });
    }

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

