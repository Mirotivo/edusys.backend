    public interface IListingService
    {
        IEnumerable<ListingDto> GetDashboardListings();
        PagedResult<ListingDto> SearchListings(string query, int page, int pageSize);
        IEnumerable<ListingDto> GetUserListings(int userId);
        ListingDto GetListingById(int id);
        Task<ListingDto> CreateListing(CreateListingDto createListingDto, int userId);
    }
