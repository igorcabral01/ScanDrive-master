namespace ScanDrive.Domain.DTOs.Reports;

/// <summary>
/// DTO para criação de test drive
/// </summary>
public class TestDriveDto
{
    /// <summary>
    /// ID do veículo para o test drive
    /// </summary>
    public string VehicleId { get; set; } = null!;

    /// <summary>
    /// Nome do cliente
    /// </summary>
    public string CustomerName { get; set; } = null!;

    /// <summary>
    /// Email do cliente
    /// </summary>
    public string CustomerEmail { get; set; } = null!;

    /// <summary>
    /// Telefone do cliente
    /// </summary>
    public string CustomerPhone { get; set; } = null!;

    /// <summary>
    /// Data e hora preferida para o test drive
    /// </summary>
    public DateTime PreferredDate { get; set; }

    /// <summary>
    /// Observações sobre o test drive
    /// </summary>
    public string? Notes { get; set; }
} 