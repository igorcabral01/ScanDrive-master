using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace ScanDrive.Domain.Entities;

public class Lead : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = null!;
    
    [Required]
    [MaxLength(20)]
    public string Phone { get; set; } = null!;
    
    [MaxLength(100)]
    public string? Email { get; set; }
    
    [Required]
    public DateTime ContactDate { get; set; }
    
    public DateTime? LastContactDate { get; set; }
    
    [MaxLength(2000)]
    public string? Notes { get; set; }
    
    [Required]
    public LeadStatus Status { get; set; }
    
    public bool HasBeenContacted { get; set; }
    
    [Required]
    public Guid ShopId { get; set; }
    public Shop? Shop { get; set; }
    
    public Guid? VehicleId { get; set; }
    public Vehicle? Vehicle { get; set; }
    
    [Required]
    public string CreatedById { get; set; } = null!;
    public IdentityUser CreatedBy { get; set; } = null!;
    
    public string? LastUpdatedById { get; set; }
    public IdentityUser? LastUpdatedBy { get; set; }
    
    public bool IsActive { get; set; } = true;
} 