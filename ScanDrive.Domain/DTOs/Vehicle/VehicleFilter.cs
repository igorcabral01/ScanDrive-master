using ScanDrive.Domain.DTOs.Common;

namespace ScanDrive.Domain.DTOs.Vehicle;

public class VehicleFilter : BaseFilter
{
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public int? Year { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? Color { get; set; }
    public string? Transmission { get; set; }
    public string? FuelType { get; set; }
    public bool? HasAuction { get; set; }
    public bool? HasAccident { get; set; }
    public bool? IsFirstOwner { get; set; }
    public bool? IsSold { get; set; }
    
    // Novos filtros baseados no JSON
    public string? Category { get; set; }
    public string? CategoryType { get; set; }
    public string? Engine { get; set; }
    public string? Version { get; set; }
    public decimal? MinOfferPrice { get; set; }
    public decimal? MaxOfferPrice { get; set; }
    public decimal? MinFipePrice { get; set; }
    public decimal? MaxFipePrice { get; set; }
    public int? Doors { get; set; }
    public string? Condition { get; set; }
    public bool? IsHighlighted { get; set; }
    public bool? IsOnOffer { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? LicensePlate { get; set; }
} 