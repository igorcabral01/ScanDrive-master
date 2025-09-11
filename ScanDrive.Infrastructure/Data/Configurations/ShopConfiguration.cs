using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ScanDrive.Domain.Entities;

namespace ScanDrive.Infrastructure.Data.Configurations;

public class ShopConfiguration : IEntityTypeConfiguration<Shop>
{
    public void Configure(EntityTypeBuilder<Shop> builder)
    {
        builder.HasQueryFilter(s => !s.IsDeleted && s.IsActive);

        builder.HasMany(s => s.Sellers)
            .WithMany()
            .UsingEntity(j => j.ToTable("ShopSellers"));
    }
} 