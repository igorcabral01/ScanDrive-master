using ScanDrive.Domain.DTOs.Common;

namespace ScanDrive.Domain.DTOs.TestDrive;

public class TestDriveFilter : BaseFilter
{
    public string? CustomerName { get; set; }
    public string? CustomerEmail { get; set; }
    public string? CustomerPhone { get; set; }
    public DateTime? PreferredDateStart { get; set; }
    public DateTime? PreferredDateEnd { get; set; }
    public bool? IsCancelled { get; set; }
    public bool? IsCompleted { get; set; }
    public Guid? VehicleId { get; set; }
    public string? VehicleBrand { get; set; }
    public string? VehicleModel { get; set; }
} 