using System.ComponentModel.DataAnnotations;

namespace ScanDrive.Domain.DTOs.Vehicle;

public class VehicleImportRequestDto
{
    /// <summary>
    /// ID da loja onde os veículos serão importados
    /// </summary>
    [Required]
    public Guid ShopId { get; set; }
    
    /// <summary>
    /// JSON com os dados dos veículos no formato base.json
    /// </summary>
    [Required]
    public string JsonData { get; set; } = null!;
} 