using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ScanDrive.Domain.Entities;

namespace ScanDrive.Infrastructure.Data.Configurations;

public class LeadConfiguration : IEntityTypeConfiguration<Lead>
{
    public void Configure(EntityTypeBuilder<Lead> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(x => x.Phone)
            .IsRequired()
            .HasMaxLength(20);
            
        builder.Property(x => x.Email)
            .HasMaxLength(100);
            
        builder.Property(x => x.ContactDate)
            .IsRequired();
            
        builder.Property(x => x.Notes)
            .HasMaxLength(2000);
            
        builder.Property(x => x.Status)
            .IsRequired();
            
        builder.HasQueryFilter(l => !l.IsDeleted && l.IsActive && l.Shop != null && l.Shop.IsActive);

        builder.HasOne(l => l.Shop)
            .WithMany()
            .HasForeignKey(l => l.ShopId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(l => l.Vehicle)
            .WithMany()
            .HasForeignKey(l => l.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(l => l.CreatedBy)
            .WithMany()
            .HasForeignKey(l => l.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(l => l.LastUpdatedBy)
            .WithMany()
            .HasForeignKey(l => l.LastUpdatedById)
            .OnDelete(DeleteBehavior.Restrict);
    }
} 