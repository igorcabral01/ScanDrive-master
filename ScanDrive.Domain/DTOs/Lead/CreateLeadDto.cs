using System.ComponentModel.DataAnnotations;

namespace ScanDrive.Domain.DTOs.Lead;

public class CreateLeadDto
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
    public Guid ShopId { get; set; }
    
    public Guid? VehicleId { get; set; }
} 