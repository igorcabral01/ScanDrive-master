using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ScanDrive.Domain.Entities;

namespace ScanDrive.Infrastructure.Data.Configurations;

public class VehicleOptionalConfiguration : IEntityTypeConfiguration<VehicleOptional>
{
    public void Configure(EntityTypeBuilder<VehicleOptional> builder)
    {
        builder.ToTable("VehicleOptionals");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.Description)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasOne(x => x.Vehicle)
            .WithMany(x => x.Optionals)
            .HasForeignKey(x => x.VehicleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.VehicleId, x.Code })
            .IsUnique();
    }
} 