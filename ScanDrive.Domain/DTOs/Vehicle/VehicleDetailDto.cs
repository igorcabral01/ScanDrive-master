namespace ScanDrive.Domain.DTOs.Vehicle;

public class VehicleDetailDto
{
    public string Id { get; set; } = null!;
    public string Brand { get; set; } = null!;
    public string Model { get; set; } = null!;
    public int Year { get; set; }
    public int Mileage { get; set; }
    public string Color { get; set; } = null!;
    public decimal Price { get; set; }
    public string Description { get; set; } = null!;
    public List<string> PhotoUrls { get; set; } = new();
    public string? MainPhotoUrl { get; set; }
    public string ShopId { get; set; } = null!;
    public string ShopName { get; set; } = null!;
    public string Transmission { get; set; } = null!;
    public string FuelType { get; set; } = null!;
    public bool HasAuction { get; set; }
    public bool HasAccident { get; set; }
    public bool IsFirstOwner { get; set; }
    public string? AuctionHistory { get; set; }
    public string? AccidentHistory { get; set; }
    public int OwnersCount { get; set; }
    public string? Features { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Novos campos baseados no JSON
    public string? ExternalVehicleCode { get; set; }
    public string? ImportCode { get; set; }
    public string? Category { get; set; }
    public string? CategoryType { get; set; }
    public string? Engine { get; set; }
    public string? Valves { get; set; }
    public string? Version { get; set; }
    public string? FullName { get; set; }
    public string? AlternativeName { get; set; }
    public decimal OfferPrice { get; set; }
    public decimal FipePrice { get; set; }
    public string? SiteObservations { get; set; }
    public string? LicensePlate { get; set; }
    public string? Renavam { get; set; }
    public int Doors { get; set; }
    public string Condition { get; set; } = "Usado";
    public bool IsHighlighted { get; set; }
    public bool IsOnOffer { get; set; }
    public string? CompanyName { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? YouTubeUrl { get; set; }
    public List<VehicleOptionalDto> Optionals { get; set; } = new();
} 