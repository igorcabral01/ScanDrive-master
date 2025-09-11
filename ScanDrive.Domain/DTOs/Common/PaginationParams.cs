using System.ComponentModel.DataAnnotations;

namespace ScanDrive.Domain.DTOs.Common;

public class PaginationParams
{
    private const int MaxPageSize = 50;
    private int _pageSize = 10;
    private int _pageNumber = 1;

    [Range(1, int.MaxValue)]
    public int PageNumber
    {
        get => _pageNumber;
        set => _pageNumber = value < 1 ? 1 : value;
    }

    [Range(1, MaxPageSize)]
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > MaxPageSize ? MaxPageSize : value < 1 ? 1 : value;
    }

    public string? OrderBy { get; set; }
    public bool IsDescending { get; set; }
} 