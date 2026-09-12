using Microsoft.EntityFrameworkCore;
using RealEstate.Infrastructure.Persistence;

namespace RealEstate.Infrastructure;

public sealed class DesignTimeDbContextFactory : Microsoft.EntityFrameworkCore.Design.IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>();
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? "Server=localhost;Database=RealEstateDb;Trusted_Connection=True;TrustServerCertificate=True;";
        options.UseSqlServer(connectionString);
        return new AppDbContext(options.Options);
    }
}
