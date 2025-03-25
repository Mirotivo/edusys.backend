using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Backend.DTOs.Listing;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SendGrid.Helpers.Errors.Model;

public class ListingService : IListingService
{
    private readonly AvanciraDbContext _dbContext;
    private readonly IFileUploadService _fileUploadService;
    private readonly ILogger<ListingService> _logger;
    private readonly IMapper _mapper; // Add AutoMapper

    public ListingService(
        AvanciraDbContext dbContext,
        IFileUploadService fileUploadService,
        ILogger<ListingService> logger,
        IMapper mapper
    )
    {
        _dbContext = dbContext;
        _fileUploadService = fileUploadService;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<PagedResult<ListingDto>> GetTutorListingsAsync(string userId, int page, int pageSize)
    {
        var query = _dbContext.Listings
            .AsNoTracking()
            .Where(l => l.UserId == userId)
            .Include(l => l.ListingLessonCategories)
                .ThenInclude(llc => llc.LessonCategory);

        var totalRecords = await query.CountAsync();

        var listings = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var listingDtos = _mapper.Map<List<ListingDto>>(listings);

        return new PagedResult<ListingDto>(listingDtos, totalRecords, page, pageSize);
    }

    public async Task<ListingDto> CreateListingAsync(ListingRequestDto model, string userId)
    {
        var existingCategoryIds = await _dbContext.LessonCategories
            .Where(c => model.CategoryIds.Contains(c.Id))
            .Select(c => c.Id)
            .ToListAsync();

        if (!existingCategoryIds.Any())
            throw new BadRequestException("No valid categories were found.");

        var listing = _mapper.Map<Listing>(model);
        listing.UserId = userId;

        listing.ListingLessonCategories = existingCategoryIds
            .Select(categoryId => new ListingLessonCategory
            {
                ListingId = listing.Id,
                LessonCategoryId = categoryId
            })
            .ToList();

        await _dbContext.Listings.AddAsync(listing);
        await _dbContext.SaveChangesAsync();

        return _mapper.Map<ListingDto>(listing);
    }

    public async Task<ListingDto> UpdateListingAsync(ListingRequestDto model, string userId)
    {
        var listing = await _dbContext.Listings
            .Include(l => l.ListingLessonCategories)
            .FirstOrDefaultAsync(l => l.Id == model.Id && l.UserId == userId);

        if (listing is null)
            throw new KeyNotFoundException("Listing not found or unauthorized.");

        var validCategoryIds = await _dbContext.LessonCategories
            .Where(c => model.CategoryIds.Contains(c.Id))
            .Select(c => c.Id)
            .ToListAsync();

        if (!validCategoryIds.Any())
            throw new ArgumentException("No valid categories found.");

        listing.ListingLessonCategories.Clear();
        listing.ListingLessonCategories = validCategoryIds
            .Select(id => new ListingLessonCategory { ListingId = listing.Id, LessonCategoryId = id })
            .ToList();

        _mapper.Map(model, listing);

        await _dbContext.SaveChangesAsync();

        return _mapper.Map<ListingDto>(listing);
    }


    public ListingDto GetListingById(int id)
    {
        var listing = _dbContext.Listings
            .Include(l => l.ListingLessonCategories).ThenInclude(l => l.LessonCategory)
            .Include(l => l.User).ThenInclude(l => l.Address)
            .FirstOrDefault(l => l.Id == id);

        return listing == null ? null : MapToListingDto(listing);
    }

    public async Task<PagedResult<ListingDto>> GetUserListingsAsync(string userId, int page, int pageSize)
    {
        var queryable = _dbContext.Listings
            .Include(l => l.ListingLessonCategories).ThenInclude(l => l.LessonCategory)
            .Include(l => l.User)
            .Where(l => l.Active && l.UserId == userId);

        // Get total count before pagination
        var totalResults = await queryable.CountAsync();

        // Apply pagination
        var lessons = await queryable
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var results = lessons.Select(listing => MapToListingDto(listing)).ToList();

        return new PagedResult<ListingDto>
        (
           results: results,
           totalResults: totalResults,
           page: page,
           pageSize: pageSize
        );
    }

    public IEnumerable<ListingDto> GetLandingPageListings()
    {
        var listings = _dbContext.Listings
            .Include(l => l.ListingLessonCategories).ThenInclude(l => l.LessonCategory)
            .Include(l => l.User)
            .Where(l => l.Active && l.IsVisible)
            .OrderBy(_ => Guid.NewGuid())
            .Take(50)
            .ToList();

        return listings.Select(l => MapToListingDto(l));
    }

    public IEnumerable<ListingDto> GetLandingPageTrendingListings()
    {
        var listings = _dbContext.Listings
            .Include(l => l.ListingLessonCategories).ThenInclude(l => l.LessonCategory)
            .Include(l => l.User)
            .Where(l => l.Active && l.IsVisible)
            .OrderBy(_ => Guid.NewGuid())
            .Take(10)
            .ToList();

        return listings.Select(l => MapToListingDto(l));
    }

    public PagedResult<ListingDto> SearchListings(string query, List<string> categories, int page, int pageSize, double? lat = null, double? lng = null, double radiusKm = 10)
    {
        var queryable = _dbContext.Listings
            .Include(l => l.ListingLessonCategories).ThenInclude(l => l.LessonCategory)
            .Include(l => l.User).ThenInclude(l => l.Address)
            .Where(l => EF.Functions.Like(l.Name, $"%{query}%") ||
                        EF.Functions.Like(l.Description, $"%{query}%"));
        // Apply category filtering if categories are selected
        if (categories.Any())
        {
            queryable = queryable.Where(l => l.ListingLessonCategories.Any(j => categories.Contains(j.LessonCategory.Name)));
        }

        if (lat.HasValue && lng.HasValue)
        {
            // Haversine formula to calculate distance in KM
            queryable = queryable.Where(l =>
                l.User.Address.Latitude != 0 && l.User.Address.Longitude != 0 &&
                (6371 * Math.Acos(
                    Math.Cos(Math.PI * lat.Value / 180) * Math.Cos(Math.PI * l.User.Address.Latitude / 180) *
                    Math.Cos(Math.PI * (lng.Value - l.User.Address.Longitude) / 180) +
                    Math.Sin(Math.PI * lat.Value / 180) * Math.Sin(Math.PI * l.User.Address.Latitude / 180)
                )) <= radiusKm);
        }

        var totalResults = queryable.Count();
        var listings = queryable
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var results = listings.Select(l => MapToListingDto(l));

        return new PagedResult<ListingDto>(
            results: results,
            totalResults: totalResults,
            page: page,
            pageSize: pageSize
        );
    }

    public ListingStatisticsDto GetListingStatistics()
    {
        var today = DateTime.UtcNow.Date;
        var startOfDay = today;
        var endOfDay = today.AddDays(1);

        var stats = _dbContext.Listings
            .AsNoTracking()
            .Where(l => l.Active && l.IsVisible)
            .GroupBy(_ => 1)
            .Select(g => new ListingStatisticsDto
            {
                TotalListings = g.Count(),
                NewListingsToday = g.Count(l => startOfDay <= l.CreatedAt && l.CreatedAt < endOfDay)
            })
            .FirstOrDefault() ?? new ListingStatisticsDto { TotalListings = 0, NewListingsToday = 0 };

        return stats;
    }

    public async Task<bool> ModifyListingTitleAsync(int listingId, string userId, string newTitle)
    {
        var listing = await _dbContext.Listings
            .Where(l => l.Id == listingId && l.UserId == userId)
            .AsTracking()
            .FirstOrDefaultAsync();

        if (listing == null) return false;

        listing.Name = newTitle;
        _dbContext.Listings.Update(listing);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ModifyListingImageAsync(int listingId, string userId, IFormFile newImage)
    {
        var listing = await _dbContext.Listings
            .Where(l => l.Id == listingId && l.UserId == userId)
            .AsTracking()
            .FirstOrDefaultAsync();

        if (listing == null) return false;

        //var imageUrl = await _fileUploadService.ReplaceFileAsync(newImage, listing.ListingImagePath, "listings");
        //listing.ListingImagePath = imageUrl;
        _dbContext.Listings.Update(listing);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ModifyListingLocationsAsync(int listingId, string userId, List<string> newLocations)
    {
        var listing = await _dbContext.Listings
            .Where(l => l.Id == listingId && l.UserId == userId)
            .AsTracking()
            .FirstOrDefaultAsync();

        if (listing == null) return false;

        // Convert list of location strings to LocationType enum
        var updatedLocations = newLocations
            .Select(location =>
            {
                if (Enum.TryParse<ListingLocationType>(location, true, out var parsedLocation))
                {
                    return parsedLocation;
                }
                return ListingLocationType.None;
            })
            .Aggregate(ListingLocationType.None, (current, parsedLocation) => current | parsedLocation);

        listing.Locations = updatedLocations;
        _dbContext.Listings.Update(listing);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ModifyListingDescriptionAsync(int listingId, string userId, string newAboutLesson, string newAboutYou)
    {
        var listing = await _dbContext.Listings
            .Where(l => l.Id == listingId && l.UserId == userId)
            .AsTracking()
            .FirstOrDefaultAsync();

        if (listing == null) return false;

        listing.Description = newAboutLesson;
        //listing.AboutYou = newAboutYou;
        _dbContext.Listings.Update(listing);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ModifyListingCategoryAsync(int listingId, string userId, int newCategoryId)
    {
        var listing = await _dbContext.Listings
            .Where(l => l.Id == listingId && l.UserId == userId)
            .AsTracking()
            .FirstOrDefaultAsync();

        if (listing == null) return false;

        var categoryExists = await _dbContext.LessonCategories.AnyAsync(c => c.Id == newCategoryId);
        if (!categoryExists) return false;

        listing.ListingLessonCategories = new List<ListingLessonCategory>
        {
            new ListingLessonCategory { LessonCategoryId = newCategoryId }
        };
        _dbContext.Listings.Update(listing);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ModifyListingRatesAsync(int listingId, string userId, RatesDto newRates)
    {
        var listing = await _dbContext.Listings
            .Where(l => l.Id == listingId && l.UserId == userId)
            .AsTracking()
            .FirstOrDefaultAsync();

        if (listing == null) return false;

        listing.HourlyRate = newRates.Hourly;
        _dbContext.Listings.Update(listing);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ToggleListingVisibilityAsync(int id)
    {
        var listing = _dbContext.Listings.AsTracking().FirstOrDefault(l => l.Id == id);
        if (listing != null)
        {
            listing.IsVisible = !listing.IsVisible;
            _dbContext.Listings.Update(listing);
            await _dbContext.SaveChangesAsync();
        }
        return true;
    }


    public async Task<bool> DeleteListingAsync(int listingId, string userId)
    {
        var listing = _dbContext.Listings
            .Where(l => l.Id == listingId && l.UserId == userId)
            .AsTracking()
            .FirstOrDefault();

        if (listing == null || !listing.Active)
        {
            return false;
        }

        listing.Active = false;
        _dbContext.Listings.Update(listing);
        await _dbContext.SaveChangesAsync();

        return true;
    }

    private ListingDto MapToListingDto(Listing listing, int? contactedCount = null)
    {
        AddressDto addressDto = null;
        var address = listing.User?.Address;
        if (address != null)
        {
            addressDto = new AddressDto
            {
                StreetAddress = address.StreetAddress,
                City = address.City,
                State = address.State,
                Country = address.Country,
                PostalCode = address.PostalCode,
                Latitude = address.Latitude,
                Longitude = address.Longitude,
                FormattedAddress = address.FormattedAddress
            };
        }

        return new ListingDto
        {
            Id = listing.Id,
            IsVisible = listing.IsVisible,
            TutorId = listing.User.Id,
            TutorName = listing.User?.FullName,
            TutorBio = listing.User?.Bio,
            ContactedCount = contactedCount ?? 0,
            TutorAddress = addressDto,
            Reviews = 0,
            LessonCategory = listing.ListingLessonCategories.FirstOrDefault()?.LessonCategory.Name,
            Title = listing.Name,
            ListingImagePath = listing.User.ProfileImageUrl,
            Locations = Enum.GetValues(typeof(ListingLocationType))
                            .Cast<ListingLocationType>()
                            .Where(location => (listing.Locations & location) == location && location != ListingLocationType.None)
                            .Select(location => location.ToString())
                            .ToList(),
            AboutLesson = listing.Description, // listing.AboutLesson,
            AboutYou = string.Empty, // listing.AboutYou,
            Rate = $"{listing.HourlyRate}/h",// $"{listing.Rates.Hourly}/h",
            Rates = new RatesDto
            {
                Hourly = listing.HourlyRate,
                FiveHours = listing.HourlyRate * 5,
                TenHours = listing.HourlyRate * 10
            },
            SocialPlatforms = new List<string> { "Messenger", "Linkedin", "Facebook", "Email" }
        };
    }

    Task<ListingResponseDto> IListingService.CreateListingAsync(ListingRequestDto model, string userId)
    {
        throw new NotImplementedException();
    }

    Task<PagedResult<ListingResponseDto>> IListingService.GetTutorListingsAsync(string userId, int page, int pageSize)
    {
        throw new NotImplementedException();
    }

    Task<ListingResponseDto> IListingService.UpdateListingAsync(ListingRequestDto model, string userId)
    {
        throw new NotImplementedException();
    }
}

