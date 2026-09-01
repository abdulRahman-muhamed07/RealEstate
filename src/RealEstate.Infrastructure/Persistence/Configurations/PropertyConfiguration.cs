using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RealEstate.Domain.Entities;

namespace RealEstate.Infrastructure.Persistence.Configurations;

public sealed class PropertyConfiguration : IEntityTypeConfiguration<Property>
{
    public void Configure(EntityTypeBuilder<Property> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Title).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Description).IsRequired().HasMaxLength(3000);
        builder.Property(x => x.Price).HasPrecision(18, 2);
        builder.Property(x => x.Type).IsRequired().HasMaxLength(100);
        builder.Property(x => x.ListingType).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.Location).IsRequired().HasMaxLength(250);
        builder.HasIndex(x => new { x.IsApproved, x.ListingType, x.CityId, x.Price });
        builder.HasIndex(x => new { x.IsApproved, x.CreatedAt });
        builder.HasOne(x => x.Owner).WithMany(x => x.Properties).HasForeignKey(x => x.OwnerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Category).WithMany(x => x.Properties).HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.City).WithMany(x => x.Properties).HasForeignKey(x => x.CityId).OnDelete(DeleteBehavior.SetNull);
        builder.HasMany(x => x.Images).WithOne(x => x.Property).HasForeignKey(x => x.PropertyId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.Bookings).WithOne(x => x.Property).HasForeignKey(x => x.PropertyId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.Reviews).WithOne(x => x.Property).HasForeignKey(x => x.PropertyId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.Favorites).WithOne(x => x.Property).HasForeignKey(x => x.PropertyId).OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class PropertyImageConfiguration : IEntityTypeConfiguration<PropertyImage>
{
    public void Configure(EntityTypeBuilder<PropertyImage> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Url).IsRequired().HasMaxLength(1000);
        builder.Property(x => x.FileName).IsRequired().HasMaxLength(200);
    }
}
