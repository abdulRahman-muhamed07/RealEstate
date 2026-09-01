using System.ComponentModel.DataAnnotations;
using RealEstate.Application.Common;
using RealEstate.Domain.Entities;

namespace RealEstate.Application.Features.Properties;

public sealed class PropertyFilterRequest
{
    [StringLength(200)] public string? Search { get; init; }
    public int? CityId { get; init; }
    public int? CategoryId { get; init; }
    public decimal? MinPrice { get; init; }
    public decimal? MaxPrice { get; init; }
    public double? MinArea { get; init; }
    public double? MaxArea { get; init; }
    public int? Bedrooms { get; init; }
    public int? Bathrooms { get; init; }
    public ListingType? ListingType { get; init; }
    public PropertyStatus? Status { get; init; }
    [StringLength(50)] public string? Type { get; init; }
    public string SortBy { get; init; } = "newest";
    [Range(1, int.MaxValue)] public int Page { get; init; } = 1;
    [Range(1, 100)] public int PageSize { get; init; } = 10;
}

public sealed class CreatePropertyRequest
{
    [Required, StringLength(200)] public string Title { get; init; } = string.Empty;
    [Required, StringLength(3000)] public string Description { get; init; } = string.Empty;
    [Range(typeof(decimal), "0.01", "79228162514264337593543950335")] public decimal Price { get; init; }
    [Range(0, double.MaxValue)] public double Area { get; init; }
    [Range(0, 100)] public int Bedrooms { get; init; }
    [Range(0, 100)] public int Bathrooms { get; init; }
    [Required, StringLength(100)] public string Type { get; init; } = "apartment";
    public ListingType ListingType { get; init; } = ListingType.Sale;
    [Required, StringLength(250)] public string Location { get; init; } = string.Empty;
    [Range(1, int.MaxValue)] public int CategoryId { get; init; }
    public int? CityId { get; init; }
    public IReadOnlyList<UploadedFile> Images { get; init; } = Array.Empty<UploadedFile>();
}

public sealed class UpdatePropertyRequest : CreatePropertyRequest { }
public sealed record ApprovePropertyRequest(bool Approve, ListingType? ListingType);
public sealed record PropertyListItem(int Id,string Title,decimal Price,double Area,int Bedrooms,int Bathrooms,string Type,ListingType ListingType,PropertyStatus Status,string Location,DateTime CreatedAt,int CategoryId,string CategoryName,int? CityId,string? CityName,string OwnerId,string OwnerName,IReadOnlyList<string> Images);
public sealed record PropertyDetails(PropertyListItem Property,double AverageRating,int ReviewCount);
public sealed record PagedResult<T>(IReadOnlyList<T> Data,int TotalCount,int Page,int PageSize,int TotalPages);
