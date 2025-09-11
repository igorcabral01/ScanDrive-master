namespace ScanDrive.Domain.DTOs.Vehicle;

public class VehicleListDto
{
    public string Id { get; set; } = null!;
    public string Brand { get; set; } = null!;
    public string Model { get; set; } = null!;
    public int Year { get; set; }
    public int Mileage { get; set; }
    public string Color { get; set; } = null!;
    public decimal Price { get; set; }
    public string? MainPhotoUrl { get; set; }
    public string ShopName { get; set; } = null!;
    public string Transmission { get; set; } = null!;
    public string FuelType { get; set; } = null!;
    public bool HasAuction { get; set; }
    public bool HasAccident { get; set; }
    public bool IsFirstOwner { get; set; }
    public int OwnersCount { get; set; }
    
    // Novos campos mais importantes para listagem
    public string? Category { get; set; }
    public string? CategoryType { get; set; }
    public string? Engine { get; set; }
    public string? Version { get; set; }
    public decimal OfferPrice { get; set; }
    public decimal FipePrice { get; set; }
    public int Doors { get; set; }
    public string Condition { get; set; } = "Usado";
    public bool IsHighlighted { get; set; }
    public bool IsOnOffer { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
} 