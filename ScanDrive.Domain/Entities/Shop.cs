using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace ScanDrive.Domain.Entities;

public class Shop : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = null!;
    
    [MaxLength(500)]
    public string Description { get; set; } = null!;
    
    [Required]
    public string OwnerId { get; set; } = null!;
    public IdentityUser Owner { get; set; } = null!;
    
    public IList<IdentityUser> Sellers { get; set; } = new List<IdentityUser>();  // ShopSellers vinculados à loja
    public IList<Vehicle> Vehicles { get; set; } = new List<Vehicle>();  // Veículos da loja
    public IList<VehicleReservation> Reservations { get; set; } = new List<VehicleReservation>();  // Reservas da loja
    public IList<TestDrive> TestDrives { get; set; } = new List<TestDrive>();  // Test drives da loja
    public IList<QRCode> QRCodes { get; set; } = new List<QRCode>();  // QR Codes da loja
    
    public bool IsActive { get; set; } = true;

    [Required]
    public int QRCodeLimit { get; set; } = 5;  // Default limit of 5 QR codes
} 