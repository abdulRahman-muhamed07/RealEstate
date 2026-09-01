using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using RealEstate.Application.Common;
using RealEstate.Application.Features.Properties;
using RealEstate.Domain.Enums;

namespace RealEstate.Api.Models;

public sealed class PropertyFormRequest
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
    public List<IFormFile> Images { get; init; } = new();

    public CreatePropertyRequest ToApplication() => new()
    {
        Title = Title,
        Description = Description,
        Price = Price,
        Area = Area,
        Bedrooms = Bedrooms,
        Bathrooms = Bathrooms,
        Type = Type,
        ListingType = ListingType,
        Location = Location,
        CategoryId = CategoryId,
        CityId = CityId,
        Images = Images.Select(x => new UploadedFile(x.FileName, x.ContentType, x.Length, x.OpenReadStream)).ToArray()
    };
}
