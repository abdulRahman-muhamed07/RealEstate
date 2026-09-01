using Microsoft.EntityFrameworkCore;
using RealEstate.Domain.Entities;

namespace RealEstate.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Property> Properties => Set<Property>();
    public DbSet<PropertyImage> PropertyImages => Set<PropertyImage>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<City> Cities => Set<City>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<Favorite> Favorites => Set<Favorite>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<User>().HasIndex(x => x.Email).IsUnique();
        b.Entity<User>().Property(x => x.Role).HasConversion<string>();
        b.Entity<Property>().Property(x => x.Price).HasPrecision(18, 2);
        b.Entity<Property>().Property(x => x.ListingType).HasConversion<string>();
        b.Entity<Property>().Property(x => x.Status).HasConversion<string>();
        b.Entity<Property>().HasIndex(x => new { x.IsApproved, x.ListingType, x.CityId, x.Price });
        b.Entity<Booking>().Property(x => x.Status).HasConversion<string>();
        b.Entity<Booking>().HasIndex(x => new { x.UserId, x.PropertyId }).IsUnique();
        b.Entity<Review>().HasIndex(x => new { x.UserId, x.PropertyId }).IsUnique();
        b.Entity<Favorite>().HasIndex(x => new { x.UserId, x.PropertyId }).IsUnique();

        b.Entity<Property>().HasOne(x => x.Owner).WithMany(x => x.Properties).HasForeignKey(x => x.OwnerId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Property>().HasOne(x => x.Category).WithMany(x => x.Properties).HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Property>().HasOne(x => x.City).WithMany(x => x.Properties).HasForeignKey(x => x.CityId).OnDelete(DeleteBehavior.SetNull);
        b.Entity<PropertyImage>().HasOne(x => x.Property).WithMany(x => x.Images).HasForeignKey(x => x.PropertyId).OnDelete(DeleteBehavior.Cascade);
        b.Entity<Booking>().HasOne(x => x.User).WithMany(x => x.Bookings).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Booking>().HasOne(x => x.Property).WithMany(x => x.Bookings).HasForeignKey(x => x.PropertyId).OnDelete(DeleteBehavior.Cascade);
        b.Entity<Review>().HasOne(x => x.User).WithMany(x => x.Reviews).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Review>().HasOne(x => x.Property).WithMany(x => x.Reviews).HasForeignKey(x => x.PropertyId).OnDelete(DeleteBehavior.Cascade);
        b.Entity<Favorite>().HasOne(x => x.User).WithMany(x => x.Favorites).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        b.Entity<Favorite>().HasOne(x => x.Property).WithMany(x => x.Favorites).HasForeignKey(x => x.PropertyId).OnDelete(DeleteBehavior.Cascade);

        b.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Apartment" }, new Category { Id = 2, Name = "Villa" },
            new Category { Id = 3, Name = "Office" }, new Category { Id = 4, Name = "Land" });
        b.Entity<City>().HasData(
            new City { Id = 1, Name = "Cairo" }, new City { Id = 2, Name = "Giza" },
            new City { Id = 3, Name = "Alexandria" }, new City { Id = 4, Name = "New Cairo" });
    }
}
