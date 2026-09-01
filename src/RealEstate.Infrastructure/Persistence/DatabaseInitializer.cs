using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RealEstate.Domain.Entities;
using RealEstate.Domain.Enums;

namespace RealEstate.Infrastructure.Persistence;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(IServiceProvider services, bool development)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var migrations = await db.Database.GetMigrationsAsync();

        if (migrations.Any())
        {
            await db.Database.MigrateAsync();
        }
        else if (development && !await db.Database.CanConnectAsync())
        {
            await db.Database.EnsureCreatedAsync();
        }

        if (development)
            await SeedAsync(db);
    }

    private static async Task SeedAsync(AppDbContext db)
    {
        if (await db.Users.AnyAsync()) return;

        var hasher = new PasswordHasher<User>();
        var admin = new User { FirstName = "System", LastName = "Admin", Email = "admin@smartrealestate.local", Role = UserRole.Admin };
        admin.PasswordHash = hasher.HashPassword(admin, "Password123!");
        var vendor = new User { FirstName = "Demo", LastName = "Vendor", Email = "vendor@smartrealestate.local", Role = UserRole.Vendor };
        vendor.PasswordHash = hasher.HashPassword(vendor, "Password123!");

        db.Users.AddRange(admin, vendor);
        db.Properties.Add(new Property
        {
            Title = "Modern Cairo Apartment",
            Description = "Demo approved property",
            Price = 2500000,
            Area = 180,
            Bedrooms = 3,
            Bathrooms = 2,
            Type = "apartment",
            ListingType = ListingType.Sale,
            Location = "New Cairo, Egypt",
            CategoryId = 1,
            CityId = 4,
            OwnerId = vendor.Id,
            IsApproved = true
        });

        await db.SaveChangesAsync();
    }
}
