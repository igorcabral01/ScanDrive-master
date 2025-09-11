using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ScanDrive.Domain.Entities;

namespace ScanDrive.Infrastructure.Data.Configurations;

public class LogConfiguration : IEntityTypeConfiguration<Log>
{
    public void Configure(EntityTypeBuilder<Log> builder)
    {
        builder.ToTable("Logs");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Level).IsRequired().HasMaxLength(50);
        builder.Property(e => e.Message).IsRequired();
        builder.Property(e => e.Source).HasMaxLength(255);
        builder.Property(e => e.Timestamp).IsRequired();
        builder.Property(e => e.UserId).HasMaxLength(450);
        builder.Property(e => e.UserName).HasMaxLength(256);
        builder.Property(e => e.RequestPath).HasMaxLength(2000);
        builder.Property(e => e.RequestMethod).HasMaxLength(10);
        builder.Property(e => e.RequestIp).HasMaxLength(50);
        builder.Property(e => e.RequestUserAgent).HasMaxLength(500);
    }
} 