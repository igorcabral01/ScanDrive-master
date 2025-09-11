namespace ScanDrive.Domain.DTOs.Vehicle;

public class VehicleReservationDto
{
    public string VehicleId { get; set; } = null!;
    public string ShopId { get; set; } = null!;
    public string? CustomerId { get; set; }
    public string CustomerName { get; set; } = null!;
    public string CustomerEmail { get; set; } = null!;
    public string CustomerPhone { get; set; } = null!;
    public DateTime ReservationDate { get; set; }
    public string? Notes { get; set; }
}

public class TestDriveDto
{
    public string VehicleId { get; set; } = null!;
    public string ShopId { get; set; } = null!;
    public string? CustomerId { get; set; }
    public string CustomerName { get; set; } = null!;
    public string CustomerEmail { get; set; } = null!;
    public string CustomerPhone { get; set; } = null!;
    public DateTime PreferredDate { get; set; }
    public string? Notes { get; set; }
} 