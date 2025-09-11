using System.ComponentModel.DataAnnotations;
using ScanDrive.Domain.Entities;
using System.Text.Json.Serialization;

namespace ScanDrive.Domain.DTOs;

public class QRCodeDTO
{
    public int Id { get; set; }
    
    [Required]
    [EnumDataType(typeof(QRCodeRedirectType))]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public QRCodeRedirectType RedirectType { get; set; }
    
    public string? RedirectUrl { get; set; }
    public string? QrCodeImageUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastUsedAt { get; set; }
    public int UsageCount { get; set; }
    public bool IsActive { get; set; }
    public int ShopId { get; set; }
    public string? ShopName { get; set; }
} 