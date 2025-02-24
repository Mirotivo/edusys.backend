using System.Diagnostics;
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

    [Authorize]
    [HttpPost("proposeLesson")]
    public async Task<IActionResult> ProposeLessonAsync([FromBody] LessonDto lessonDto)
    {
        var userId = GetUserId();
        var result = await _lessonService.ProposeLessonAsync(lessonDto, userId);
        return JsonOk(new { Message = "Lesson proposed successfully.", Lesson = result });
    }

    [Authorize]
    [HttpPost("respondToProposition/{lessonId}")]
    public async Task<IActionResult> RespondToPropositionAsync(int lessonId, [FromBody] bool accept)
    {
        var userId = GetUserId();

        if (!await _lessonService.RespondToPropositionAsync(lessonId, accept, userId))
        {
            return NotFound(new { Message = "Lesson not found." });
        }

        return JsonOk(new { Message = accept ? "Proposition accepted." : "Proposition refused." });
    }

    [Authorize]
    [HttpGet("{contactId}/{listingId}")]
    public async Task<IActionResult> GetLessonsAsync(string contactId, int listingId)
    {
        var userId = GetUserId();

        // Call the separate service methods
        var propositions = await _lessonService.GetLessonPropositionsAsync(contactId, userId, listingId);
        var lessons = await _lessonService.GetLessonsAsync(contactId, userId, listingId);

        // Return a combined result
        return JsonOk(new { Propositions = propositions, Lessons = lessons });
    }

    [Authorize]
    [HttpGet()]
    public async Task<IActionResult> GetAllLessonsAsync()
    {
        var userId = GetUserId();

        var propositions = await _lessonService.GetAllLessonPropositionsAsync(userId);
        var lessons = await _lessonService.GetAllLessonsAsync(userId);

        return JsonOk(new { Propositions = propositions, Lessons = lessons });
    }

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

