using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RealEstate.Domain.Entities;

namespace RealEstate.Infrastructure.Persistence.Configurations;

public sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        builder.HasIndex(x => x.Name).IsUnique();
        builder.HasData(
            new Category { Id = 1, Name = "Apartment" },
            new Category { Id = 2, Name = "Villa" },
            new Category { Id = 3, Name = "Office" },
            new Category { Id = 4, Name = "Land" });
    }
}

public sealed class CityConfiguration : IEntityTypeConfiguration<City>
{
    public void Configure(EntityTypeBuilder<City> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        builder.HasIndex(x => x.Name).IsUnique();
        builder.HasData(
            new City { Id = 1, Name = "Cairo" },
            new City { Id = 2, Name = "Giza" },
            new City { Id = 3, Name = "Alexandria" },
            new City { Id = 4, Name = "New Cairo" });
    }
}
