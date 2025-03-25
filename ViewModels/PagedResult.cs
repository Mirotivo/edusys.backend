using System.Collections.Generic;
using System;
using System.Linq;

public class PagedResult<T>
{
    public int TotalResults { get; }
    public int Page { get; }
    public int PageSize { get; }
    public IEnumerable<T> Results { get; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalResults / PageSize) : 0;

    public PagedResult(IEnumerable<T> results, int totalResults, int page, int pageSize)
    {
        TotalResults = Math.Max(0, totalResults);
        PageSize = Math.Max(1, pageSize);
        Page = Math.Max(1, page);
        Results = results?.ToList() ?? new List<T>();
    }
}
