using System.ComponentModel.DataAnnotations;
using ScanDrive.Domain.Entities;

namespace ScanDrive.Domain.DTOs.Lead;

public class UpdateLeadDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = null!;
    
    [Required]
    [MaxLength(20)]
    public string Phone { get; set; } = null!;
    
    [MaxLength(100)]
    [EmailAddress]
    public string? Email { get; set; }
    
    [MaxLength(2000)]
    public string? Notes { get; set; }
    
    [Required]
    public LeadStatus Status { get; set; }
    
    public bool HasBeenContacted { get; set; }
    
    public Guid? VehicleId { get; set; }
    
    public bool IsActive { get; set; }
} 