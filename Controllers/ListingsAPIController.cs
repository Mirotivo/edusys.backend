using System.Diagnostics;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace avancira.Controllers;

[Route("api/listings")]
[ApiController]
public class ListingsAPIController : BaseController
{
    private readonly IListingService _listingService;

    public ListingsAPIController(
        IListingService listingService
    )
    {
        _listingService = listingService;
    }

    [HttpGet("Dashboard")]
    public IActionResult Dashboard()
    {
        var listings = _listingService.GetDashboardListings();
        if (!listings.Any())
        {
            return NotFound("No listings found.");
        }

        return JsonOk(listings);
    }

    [HttpGet("search")]
    public IActionResult Search([FromQuery] string query, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return JsonError("Search query cannot be empty.");
        }

        var results = _listingService.SearchListings(query, page, pageSize);
        return JsonOk(results);
    }


    [Authorize]
    [HttpGet()]
    public IActionResult Get()
    {
        var userId = GetUserId();
        var listings = _listingService.GetUserListings(userId);
        return JsonOk(listings);
    }


    [HttpGet("{id:int}")]
    public IActionResult GetListingById(int id)
    {
        var listing = _listingService.GetListingById(id);
        if (listing == null)
        {
            return NotFound($"Listing with ID {id} not found.");
        }

        return JsonOk(listing);
    }


    [Authorize]
    [HttpPost("create-listing")]
    public async Task<IActionResult> Create([FromForm] CreateListingDto createListingDto)
    {
        try
        {
            if (createListingDto == null)
            {
                return JsonError("Invalid data.");
            }

            var userId = GetUserId();
            var listing = await _listingService.CreateListing(createListingDto, userId);
            return CreatedAtAction(nameof(GetListingById), new { id = listing.Id }, listing);
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                Message = "An error occurred while creating the listing.",
                Error = ex.Message
            });
        }
    }

}

