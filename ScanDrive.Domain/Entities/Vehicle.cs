using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace ScanDrive.Domain.Entities;

public class Vehicle : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Brand { get; set; } = null!;
    
    [Required]
    [MaxLength(100)]
    public string Model { get; set; } = null!;
    
    [Required]
    public int Year { get; set; }
    
    [Required]
    public decimal Price { get; set; }
    
    [MaxLength(2000)]
    public string Description { get; set; } = null!;
    
    public int Mileage { get; set; }
    
    [MaxLength(100)]
    public string Color { get; set; } = null!;
    
    [MaxLength(50)]
    public string Transmission { get; set; } = "Manual";
    
    [MaxLength(50)]
    public string FuelType { get; set; } = "Gasolina";
    
    public bool HasAuction { get; set; }
    
    public bool HasAccident { get; set; }
    
    public bool IsFirstOwner { get; set; }
    
    [MaxLength(2000)]
    public string? AuctionHistory { get; set; }
    
    [MaxLength(2000)]
    public string? AccidentHistory { get; set; }
    
    public int OwnersCount { get; set; } = 1;
    
    [MaxLength(2000)]
    public string? Features { get; set; }
    
    public string? MainPhotoUrl { get; set; }
    
    [Required]
    public Guid ShopId { get; set; }
    public Shop? Shop { get; set; }
    
    [Required]
    public string CreatedById { get; set; } = null!;
    public IdentityUser CreatedBy { get; set; } = null!;
    
    public string? LastUpdatedById { get; set; }
    public IdentityUser? LastUpdatedBy { get; set; }
    
    public bool IsActive { get; set; } = true;
    public bool IsSold { get; set; } = false;
    
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
    
    public decimal OfferPrice { get; set; } = 0;
    
    public decimal FipePrice { get; set; } = 0;
    
    [MaxLength(4000)]
    public string? SiteObservations { get; set; }
    
    [MaxLength(20)]
    public string? LicensePlate { get; set; }
    
    [MaxLength(50)]
    public string? Renavam { get; set; }
    
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
    
    public virtual IList<VehiclePhoto> Photos { get; set; } = new List<VehiclePhoto>();
    public virtual IList<VehicleReservation> Reservations { get; set; } = new List<VehicleReservation>();
    public virtual IList<TestDrive> TestDrives { get; set; } = new List<TestDrive>();
    public virtual IList<VehicleOptional> Optionals { get; set; } = new List<VehicleOptional>();
} 