using System.ComponentModel.DataAnnotations;
using ScanDrive.Domain.Entities;
using System.Text.Json.Serialization;

namespace ScanDrive.Domain.DTOs;

public class CreateQRCodeDTO
{
    [Required]
    [EnumDataType(typeof(QRCodeRedirectType))]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public QRCodeRedirectType RedirectType { get; set; }
    
    public int ShopId { get; set; }
} 