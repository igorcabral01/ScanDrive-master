using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ScanDrive.Domain.Entities;

namespace ScanDrive.Infrastructure.Data.Configurations;

public class VehicleReservationConfiguration : IEntityTypeConfiguration<VehicleReservation>
{
    public void Configure(EntityTypeBuilder<VehicleReservation> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.CustomerName)
            .IsRequired()
            .HasMaxLength(200);
            
        builder.Property(x => x.CustomerEmail)
            .IsRequired()
            .HasMaxLength(256);
            
        builder.Property(x => x.CustomerPhone)
            .IsRequired()
            .HasMaxLength(20);
            
        builder.Property(x => x.Notes)
            .HasMaxLength(2000);
            
        builder.Property(x => x.CancellationReason)
            .HasMaxLength(2000);
            
        builder.HasQueryFilter(r => !r.IsDeleted && r.IsActive && r.Shop != null && r.Shop.IsActive);

        builder.HasOne(r => r.Vehicle)
            .WithMany(v => v.Reservations)
            .HasForeignKey(r => r.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Shop)
            .WithMany(s => s.Reservations)
            .HasForeignKey(r => r.ShopId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Customer)
            .WithMany()
            .HasForeignKey(r => r.CustomerId)
            .OnDelete(DeleteBehavior.SetNull);
    }
} 