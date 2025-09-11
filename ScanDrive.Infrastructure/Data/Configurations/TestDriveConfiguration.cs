using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ScanDrive.Domain.Entities;

namespace ScanDrive.Infrastructure.Data.Configurations;

public class TestDriveConfiguration : IEntityTypeConfiguration<TestDrive>
{
    public void Configure(EntityTypeBuilder<TestDrive> builder)
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
            
        builder.Property(x => x.CompletionNotes)
            .HasMaxLength(2000);
            
        builder.Property(x => x.CancellationReason)
            .HasMaxLength(2000);
            
        builder.HasOne(x => x.Vehicle)
            .WithMany(x => x.TestDrives)
            .HasForeignKey(x => x.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasOne(x => x.Shop)
            .WithMany(x => x.TestDrives)
            .HasForeignKey(x => x.ShopId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasOne(x => x.Customer)
            .WithMany()
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.SetNull);
    }
} 