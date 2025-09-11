using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ScanDrive.Domain.Entities;
using ScanDrive.Infrastructure.Data.Configurations;

namespace ScanDrive.Infrastructure.Context;

public class AppDbContext : IdentityDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Shop> Shops { get; set; } = null!;
    public DbSet<Vehicle> Vehicles { get; set; } = null!;
    public DbSet<VehiclePhoto> VehiclePhotos { get; set; } = null!;
    public DbSet<VehicleOptional> VehicleOptionals { get; set; } = null!;
    public DbSet<VehicleReservation> VehicleReservations { get; set; } = null!;
    public DbSet<TestDrive> TestDrives { get; set; } = null!;
    public DbSet<ChatSession> ChatSessions { get; set; } = null!;
    public DbSet<ChatMessage> ChatMessages { get; set; } = null!;
    public DbSet<ChatQuestion> ChatQuestions { get; set; } = null!;
    public DbSet<Lead> Leads { get; set; } = null!;
    public DbSet<QRCode> QRCodes { get; set; } = null!;
    public DbSet<Log> Logs { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Aplicar configurações
        builder.ApplyConfiguration(new ShopConfiguration());
        builder.ApplyConfiguration(new VehicleConfiguration());
        builder.ApplyConfiguration(new VehicleOptionalConfiguration());
        builder.ApplyConfiguration(new LeadConfiguration());
        builder.ApplyConfiguration(new TestDriveConfiguration());
        builder.ApplyConfiguration(new VehicleReservationConfiguration());
        builder.ApplyConfiguration(new ChatConfiguration());
        builder.ApplyConfiguration(new ChatMessageConfiguration());
        builder.ApplyConfiguration(new ChatQuestionConfiguration());
        builder.ApplyConfiguration(new QRCodeConfiguration());
        builder.ApplyConfiguration(new LogConfiguration());
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseNpgsql(
                "Host=scandrivedba.c9emc62cgw4d.us-east-2.rds.amazonaws.com;Port=5432;Database=postgres;Username=postgres;Password=8kL8bsi9RY7ogl5U"
            );
        }
    }
} 