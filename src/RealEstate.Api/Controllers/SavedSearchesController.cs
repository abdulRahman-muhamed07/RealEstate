using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealEstate.Api.Extensions;
using RealEstate.Application.Features.SavedSearches;
using RealEstate.Application.Interfaces;

namespace RealEstate.Api.Controllers;

[ApiController]
[Route("api/saved-searches")]
[Authorize]
public sealed class SavedSearchesController(ISavedSearchService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Mine(CancellationToken ct) => Ok(await service.GetMineAsync(UserId(), ct));

    [HttpPost]
    public async Task<IActionResult> Create(CreateSavedSearchRequest request, CancellationToken ct) =>
        this.ToActionResult(await service.CreateAsync(request, UserId(), ct));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct) =>
        this.ToActionResult(await service.DeleteAsync(id, UserId(), ct));

    private string UserId() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException();
}
