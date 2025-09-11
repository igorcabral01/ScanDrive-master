namespace ScanDrive.Domain.DTOs.TestDrive;

public class TestDriveListItemDTO
{
    public Guid Id { get; set; }
    public string CustomerName { get; set; } = null!;
    public string CustomerEmail { get; set; } = null!;
    public string CustomerPhone { get; set; } = null!;
    public DateTime PreferredDate { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
    public bool IsCancelled { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletionDate { get; set; }
    public string? CompletionNotes { get; set; }
    public DateTime? CancellationDate { get; set; }
    public string? CancellationReason { get; set; }

    // Vehicle Info
    public Guid VehicleId { get; set; }
    public string VehicleBrand { get; set; } = null!;
    public string VehicleModel { get; set; } = null!;
    public int VehicleYear { get; set; }
    public string? VehicleMainPhotoUrl { get; set; }
    public string VehicleTransmission { get; set; } = null!;
    public string VehicleFuelType { get; set; } = null!;
    public bool VehicleHasAuction { get; set; }
    public bool VehicleHasAccident { get; set; }
    public bool VehicleIsFirstOwner { get; set; }
    public int VehicleOwnersCount { get; set; }

    // Shop Info
    public Guid ShopId { get; set; }
    public string ShopName { get; set; } = null!;
}