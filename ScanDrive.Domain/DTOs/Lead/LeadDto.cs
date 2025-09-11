using ScanDrive.Domain.Entities;

namespace ScanDrive.Domain.DTOs.Lead;

public class LeadDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string? Email { get; set; }
    public DateTime ContactDate { get; set; }
    public DateTime? LastContactDate { get; set; }
    public string? Notes { get; set; }
    public LeadStatus Status { get; set; }
    public bool HasBeenContacted { get; set; }
    public Guid ShopId { get; set; }
    public string ShopName { get; set; } = null!;
    public Guid? VehicleId { get; set; }
    public string? VehicleName { get; set; }
    public string CreatedById { get; set; } = null!;
    public string CreatedByName { get; set; } = null!;
    public string? LastUpdatedById { get; set; }
    public string? LastUpdatedByName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; }
} 