using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ScanDrive.Domain.Entities;

namespace ScanDrive.Infrastructure.Data.Configurations;

public class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.HasQueryFilter(v => !v.IsDeleted && v.IsActive && v.Shop != null && v.Shop.IsActive);

        builder.HasOne(v => v.Shop)
            .WithMany(s => s.Vehicles)
            .HasForeignKey(v => v.ShopId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(v => v.CreatedBy)
            .WithMany()
            .HasForeignKey(v => v.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.LastUpdatedBy)
            .WithMany()
            .HasForeignKey(x => x.LastUpdatedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Photos)
            .WithOne(x => x.Vehicle)
            .HasForeignKey(x => x.VehicleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Reservations)
            .WithOne(x => x.Vehicle)
            .HasForeignKey(x => x.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.TestDrives)
            .WithOne(x => x.Vehicle)
            .HasForeignKey(x => x.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Brand)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(x => x.Model)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(x => x.Year)
            .IsRequired();
            
        builder.Property(x => x.Price)
            .IsRequired()
            .HasPrecision(18, 2);
            
        builder.Property(x => x.Description)
            .HasMaxLength(2000);
            
        builder.Property(x => x.Color)
            .HasMaxLength(100);
            
        builder.Property(x => x.Transmission)
            .HasMaxLength(50)
            .HasDefaultValue("Manual");
            
        builder.Property(x => x.FuelType)
            .HasMaxLength(50)
            .HasDefaultValue("Gasolina");
            
        builder.Property(x => x.HasAuction)
            .HasDefaultValue(false);
            
        builder.Property(x => x.HasAccident)
            .HasDefaultValue(false);
            
        builder.Property(x => x.IsFirstOwner)
            .HasDefaultValue(true);
            
        builder.Property(x => x.AuctionHistory)
            .HasMaxLength(2000);
            
        builder.Property(x => x.AccidentHistory)
            .HasMaxLength(2000);
            
        builder.Property(x => x.OwnersCount)
            .HasDefaultValue(1);
            
        builder.Property(x => x.Features)
            .HasMaxLength(2000);
            
        builder.Property(x => x.MainPhotoUrl)
            .HasMaxLength(500);
    }
} 