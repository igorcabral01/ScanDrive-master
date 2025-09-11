namespace ScanDrive.Domain.DTOs.TestDrive;

public class CreateTestDriveDTO
{
    public Guid VehicleId { get; set; }
    public string CustomerName { get; set; } = null!;
    public string CustomerEmail { get; set; } = null!;
    public string CustomerPhone { get; set; } = null!;
    public DateTime PreferredDate { get; set; }
    public string? Notes { get; set; }
}