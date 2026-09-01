using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealEstate.Api.Extensions;
using RealEstate.Application.Features.Reviews;
using RealEstate.Application.Interfaces;

namespace RealEstate.Api.Controllers;

[ApiController]
[Route("api/reviews")]
public sealed class ReviewsController(IReviewService service) : ControllerBase
{
    [HttpGet("property/{propertyId:int}")]
    public async Task<IActionResult> GetForProperty(int propertyId, CancellationToken ct) => Ok(await service.GetForPropertyAsync(propertyId, ct));

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create(CreateReviewRequest request, CancellationToken ct) =>
        this.ToActionResult(await service.CreateAsync(request, UserId(), ct));

    [Authorize]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateReviewRequest request, CancellationToken ct) =>
        this.ToActionResult(await service.UpdateAsync(id, request, UserId(), ct));

    [Authorize]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct) =>
        this.ToActionResult(await service.DeleteAsync(id, UserId(), User.IsInRole("Admin"), ct));

    [Authorize]
    [HttpGet("my-reviews")]
    public async Task<IActionResult> Mine(CancellationToken ct) => Ok(await service.GetMineAsync(UserId(), ct));

    private string UserId() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException();
}
