using ScanDrive.Domain.DTOs.Common;

namespace ScanDrive.Domain.DTOs.VehicleReservation;

public class VehicleReservationFilter : BaseFilter
{
    public string? CustomerName { get; set; }
    public string? CustomerEmail { get; set; }
    public string? CustomerPhone { get; set; }
    public DateTime? ReservationDateStart { get; set; }
    public DateTime? ReservationDateEnd { get; set; }
    public bool? IsCancelled { get; set; }
    public Guid? VehicleId { get; set; }
    public string? VehicleBrand { get; set; }
    public string? VehicleModel { get; set; }
} 