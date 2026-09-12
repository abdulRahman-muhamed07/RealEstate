using System.ComponentModel.DataAnnotations;
using RealEstate.Domain.Enums;

namespace RealEstate.Application.Features.SavedSearches;

public sealed record CreateSavedSearchRequest(
    [property:Required, StringLength(100)] string Name,
    int? CityId,
    int? CategoryId,
    decimal? MinPrice,
    decimal? MaxPrice,
    int? Bedrooms,
    ListingType? ListingType,
    [property:StringLength(100)] string? Type);

public sealed record SavedSearchDto(int Id, string Name, int? CityId, int? CategoryId, decimal? MinPrice, decimal? MaxPrice, int? Bedrooms, ListingType? ListingType, string? Type, bool IsActive);
