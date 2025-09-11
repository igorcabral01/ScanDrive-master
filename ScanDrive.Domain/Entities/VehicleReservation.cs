using Microsoft.AspNetCore.Identity;

namespace ScanDrive.Domain.Entities;

public class VehicleReservation : BaseEntity
{
    public Guid VehicleId { get; set; }
    public Vehicle? Vehicle { get; set; }

    public Guid ShopId { get; set; }
    public Shop? Shop { get; set; }

    public string? CustomerId { get; set; }
    public IdentityUser? Customer { get; set; }

    public string CustomerName { get; set; } = null!;
    public string CustomerEmail { get; set; } = null!;
    public string CustomerPhone { get; set; } = null!;
    public DateTime ReservationDate { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsCancelled { get; set; } = false;
    public DateTime? CancellationDate { get; set; }
    public string? CancellationReason { get; set; }
} 