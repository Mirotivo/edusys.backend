    public class PagedResult<T>
    {
        /// <summary>
        /// The total number of results across all pages.
        /// </summary>
        public int TotalResults { get; set; }

        /// <summary>
        /// The current page number.
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// The number of items per page.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// The collection of results for the current page.
        /// </summary>
        public IEnumerable<T> Results { get; set; } = new List<T>();

        /// <summary>
        /// The total number of pages based on the total results and page size.
        /// </summary>
        public int TotalPages => (int)Math.Ceiling((double)TotalResults / PageSize);
    }
