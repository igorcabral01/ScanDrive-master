namespace ScanDrive.Domain.DTOs.Common;

public class BaseFilter : PaginationParams
{
    public string? Search { get; set; }
    public bool? IsActive { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public Guid? ShopId { get; set; }
} 