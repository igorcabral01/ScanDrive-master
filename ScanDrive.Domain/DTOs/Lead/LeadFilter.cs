using ScanDrive.Domain.DTOs.Common;
using ScanDrive.Domain.Entities;

namespace ScanDrive.Domain.DTOs.Lead;

public class LeadFilter : BaseFilter
{
    public string? CustomerName { get; set; }
    public string? CustomerEmail { get; set; }
    public string? CustomerPhone { get; set; }
    public DateTime? ContactDateStart { get; set; }
    public DateTime? ContactDateEnd { get; set; }
    public DateTime? LastContactDateStart { get; set; }
    public DateTime? LastContactDateEnd { get; set; }
    public bool? HasBeenContacted { get; set; }
    public LeadStatus? Status { get; set; }
    public Guid? VehicleId { get; set; }
    public string? VehicleBrand { get; set; }
    public string? VehicleModel { get; set; }
} 