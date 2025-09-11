using System.ComponentModel.DataAnnotations;

namespace ScanDrive.Domain.DTOs.Vehicle;

public class CreateVehicleDto
{
    [Required]
    [MaxLength(100)]
    public string Brand { get; set; } = null!;
    
    [Required]
    [MaxLength(100)]
    public string Model { get; set; } = null!;
    
    [Required]
    [Range(1900, 2100)]
    public int Year { get; set; }
    
    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Price { get; set; }
    
    [MaxLength(2000)]
    public string? Description { get; set; }
    
    [Range(0, int.MaxValue)]
    public int Mileage { get; set; }
    
    [MaxLength(100)]
    public string? Color { get; set; }
    
    [MaxLength(50)]
    public string? Transmission { get; set; }
    
    [MaxLength(50)]
    public string? FuelType { get; set; }
    
    public bool HasAuction { get; set; }
    
    public bool HasAccident { get; set; }
    
    public bool IsFirstOwner { get; set; }
    
    [MaxLength(2000)]
    public string? AuctionHistory { get; set; }
    
    [MaxLength(2000)]
    public string? AccidentHistory { get; set; }
    
    [Range(1, int.MaxValue)]
    public int OwnersCount { get; set; } = 1;
    
    [MaxLength(2000)]
    public string? Features { get; set; }
    
    [Required]
    public Guid ShopId { get; set; }
    
    // Novos campos baseados no JSON
    [MaxLength(200)]
    public string? ExternalVehicleCode { get; set; }
    
    [MaxLength(200)]
    public string? ImportCode { get; set; }
    
    [MaxLength(100)]
    public string? Category { get; set; }
    
    [MaxLength(100)]
    public string? CategoryType { get; set; }
    
    [MaxLength(50)]
    public string? Engine { get; set; }
    
    [MaxLength(20)]
    public string? Valves { get; set; }
    
    [MaxLength(200)]
    public string? Version { get; set; }
    
    [MaxLength(300)]
    public string? FullName { get; set; }
    
    [MaxLength(300)]
    public string? AlternativeName { get; set; }
    
    [Range(0, double.MaxValue)]
    public decimal OfferPrice { get; set; } = 0;
    
    [Range(0, double.MaxValue)]
    public decimal FipePrice { get; set; } = 0;
    
    [MaxLength(4000)]
    public string? SiteObservations { get; set; }
    
    [MaxLength(20)]
    public string? LicensePlate { get; set; }
    
    [MaxLength(50)]
    public string? Renavam { get; set; }
    
    [Range(1, 10)]
    public int Doors { get; set; } = 4;
    
    [MaxLength(20)]
    public string Condition { get; set; } = "Usado";
    
    public bool IsHighlighted { get; set; } = false;
    
    public bool IsOnOffer { get; set; } = false;
    
    [MaxLength(200)]
    public string? CompanyName { get; set; }
    
    [MaxLength(100)]
    public string? City { get; set; }
    
    [MaxLength(2)]
    public string? State { get; set; }
    
    [MaxLength(500)]
    public string? YouTubeUrl { get; set; }
    
    public List<VehicleOptionalDto> Optionals { get; set; } = new();
} 