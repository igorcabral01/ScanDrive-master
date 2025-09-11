using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace ScanDrive.Domain.Entities;

public class QRCode : BaseEntity
{
    [Required]
    public Guid ShopId { get; set; }

    [ForeignKey(nameof(ShopId))]
    public Shop? Shop { get; set; }

    [Required]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public QRCodeRedirectType RedirectType { get; set; }

    [Required]
    public Guid RedirectId { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime? LastUsedAt { get; set; }
}

/// <summary>
/// Tipos de redirecionamento possíveis para o QR Code.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum QRCodeRedirectType
{
    /// <summary>
    /// Redireciona para a página da loja.
    /// </summary>
    [Description("Redireciona para a página da loja.")]
    Shop = 0,
    /// <summary>
    /// Redireciona para a página do veículo.
    /// </summary>
    [Description("Redireciona para a página do veículo.")]
    Vehicle = 1
} 