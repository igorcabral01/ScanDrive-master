namespace ScanDrive.Domain.Entities;

public class VehiclePhoto : BaseEntity
{
    public string Url { get; set; } = null!;
    public string FileName { get; set; } = null!;
    public int Order { get; set; }
    public bool IsMain { get; set; }
    
    public Guid VehicleId { get; set; }
    public Vehicle? Vehicle { get; set; }
} 