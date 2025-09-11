namespace ScanDrive.Domain.DTOs.Common;

public class PagedList<T>
{
    public IEnumerable<T> Items { get; set; } = new List<T>();
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public int TotalCount { get; set; }
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    public PagedList(IEnumerable<T> items, int count, PaginationParams paginationParams)
    {
        PageNumber = paginationParams.PageNumber;
        PageSize = paginationParams.PageSize;
        TotalCount = count;
        TotalPages = (int)Math.Ceiling(count / (double)PageSize);
        Items = items;
    }
} 