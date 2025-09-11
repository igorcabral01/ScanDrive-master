using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ScanDrive.Infrastructure.Context;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql(
            "Host=scandrivedba.c9emc62cgw4d.us-east-2.rds.amazonaws.com;Port=5432;Database=postgres;Username=postgres;Password=8kL8bsi9RY7ogl5U"
        );

        return new AppDbContext(optionsBuilder.Options);
    }
} 