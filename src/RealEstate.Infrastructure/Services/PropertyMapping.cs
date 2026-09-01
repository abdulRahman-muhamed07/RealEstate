using System.Linq.Expressions;
using RealEstate.Application.Features.Properties;
using RealEstate.Domain.Entities;

namespace RealEstate.Infrastructure.Services;

internal static class PropertyMapping
{
    public static PropertyListItem Map(Property property) => new(
        property.Id, property.Title, property.Price, property.Area, property.Bedrooms, property.Bathrooms, property.Type,
        property.ListingType, property.Status, property.Location, property.CreatedAt, property.CategoryId,
        property.Category?.Name ?? string.Empty, property.CityId, property.City?.Name, property.OwnerId,
        $"{property.Owner?.FirstName} {property.Owner?.LastName}".Trim(), property.Images.Select(x => x.Url).ToList());

    public static Expression<Func<Property, PropertyListItem>> Projection() => property => new PropertyListItem(
        property.Id, property.Title, property.Price, property.Area, property.Bedrooms, property.Bathrooms, property.Type,
        property.ListingType, property.Status, property.Location, property.CreatedAt, property.CategoryId, property.Category.Name,
        property.CityId, property.City == null ? null : property.City.Name, property.OwnerId,
        (property.Owner.FirstName + " " + property.Owner.LastName).Trim(), property.Images.Select(image => image.Url).ToList());
}
