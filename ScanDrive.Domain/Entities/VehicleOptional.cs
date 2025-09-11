using System.ComponentModel.DataAnnotations;

namespace ScanDrive.Domain.Entities;

public class VehicleOptional : BaseEntity
{
    [Required]
    [MaxLength(20)]
    public string Code { get; set; } = null!;
    
    [Required]
    [MaxLength(200)]
    public string Description { get; set; } = null!;
    
    [Required]
    public Guid VehicleId { get; set; }
    public Vehicle? Vehicle { get; set; }
} 