namespace ScanDrive.Domain.DTOs.Reports;

/// <summary>
/// DTO para criação de reserva de veículo
/// </summary>
public class VehicleReservationDto
{
    /// <summary>
    /// ID do veículo a ser reservado
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
    /// Data da reserva
    /// </summary>
    public DateTime ReservationDate { get; set; }

    /// <summary>
    /// Observações sobre a reserva
    /// </summary>
    public string? Notes { get; set; }
} 