using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealEstate.Api.Extensions;
using RealEstate.Application.Features.Favorites;
using RealEstate.Application.Interfaces;

namespace RealEstate.Api.Controllers;

[ApiController]
[Route("api/favorites")]
[Authorize]
public sealed class FavoritesController(IFavoriteService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct) => Ok(await service.GetMineAsync(UserId(), ct));

    [HttpPost]
    public async Task<IActionResult> Add(FavoriteRequest request, CancellationToken ct) =>
        this.ToActionResult(await service.AddAsync(request.PropertyId, UserId(), ct));

    [HttpDelete("{propertyId:int}")]
    public async Task<IActionResult> Remove(int propertyId, CancellationToken ct) =>
        this.ToActionResult(await service.RemoveAsync(propertyId, UserId(), ct));

    private string UserId() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException();
}
