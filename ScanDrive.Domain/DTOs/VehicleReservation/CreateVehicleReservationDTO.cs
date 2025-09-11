namespace ScanDrive.Domain.DTOs.VehicleReservation;

public class CreateVehicleReservationDTO
{
    public Guid VehicleId { get; set; }
    public string CustomerName { get; set; } = null!;
    public string CustomerEmail { get; set; } = null!;
    public string CustomerPhone { get; set; } = null!;
    public DateTime ReservationDate { get; set; }
    public string? Notes { get; set; }
}