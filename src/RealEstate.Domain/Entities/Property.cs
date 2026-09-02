using System.ComponentModel.DataAnnotations;
using RealEstate.Domain.Enums;

namespace RealEstate.Domain.Entities;

public sealed class Property
{
    public int Id { get; set; }
    [MaxLength(200)] public string Title { get; set; } = string.Empty;
    [MaxLength(3000)] public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public double Area { get; set; }
    public int Bedrooms { get; set; }
    public int Bathrooms { get; set; }
    [MaxLength(100)] public string Type { get; set; } = "apartment";
    public ListingType ListingType { get; set; } = ListingType.Sale;
    public PropertyStatus Status { get; set; } = PropertyStatus.Available;
    [MaxLength(250)] public string Location { get; set; } = string.Empty;
    public bool IsApproved { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string OwnerId { get; set; } = string.Empty;
    public User Owner { get; set; } = null!;
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    public int? CityId { get; set; }
    public City? City { get; set; }
    public ICollection<PropertyImage> Images { get; set; } = new List<PropertyImage>();
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();

    public bool CanBeModifiedBy(string userId, bool isAdmin) => isAdmin || OwnerId == userId;

    public void UpdateDetails(
        string title,
        string description,
        decimal price,
        double area,
        int bedrooms,
        int bathrooms,
        string type,
        ListingType listingType,
        string location,
        int categoryId,
        int? cityId,
        bool approved)
    {
        Title = title.Trim();
        Description = description.Trim();
        Price = price;
        Area = area;
        Bedrooms = bedrooms;
        Bathrooms = bathrooms;
        Type = type.Trim().ToLowerInvariant();
        ListingType = listingType;
        Location = location.Trim();
        CategoryId = categoryId;
        CityId = cityId;
        IsApproved = approved;
        Status = PropertyStatus.Available;
    }

    public void Approve(ListingType? listingType = null)
    {
        IsApproved = true;
        if (listingType.HasValue)
            ListingType = listingType.Value;
        Status = PropertyStatus.Available;
    }
}

public sealed class PropertyImage
{
    public int Id { get; set; }
    [MaxLength(1000)] public string Url { get; set; } = string.Empty;
    [MaxLength(200)] public string FileName { get; set; } = string.Empty;
    public int PropertyId { get; set; }
    public Property Property { get; set; } = null!;
}
