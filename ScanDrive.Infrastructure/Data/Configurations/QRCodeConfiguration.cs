using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ScanDrive.Domain.Entities;

namespace ScanDrive.Infrastructure.Data.Configurations;

public class QRCodeConfiguration : IEntityTypeConfiguration<QRCode>
{
    public void Configure(EntityTypeBuilder<QRCode> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.RedirectType).IsRequired();

        builder.HasOne(e => e.Shop)
            .WithMany(s => s.QRCodes)
            .HasForeignKey(e => e.ShopId)
            .OnDelete(DeleteBehavior.Restrict);
    }
} 