namespace RealEstate.Application.Features.Favorites;

public sealed record FavoriteRequest(int PropertyId);
public sealed record FavoriteDto(int PropertyId,string Title,decimal Price,string? ImageUrl,string Location);
