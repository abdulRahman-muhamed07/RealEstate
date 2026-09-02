using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealEstate.Api.Extensions;
using RealEstate.Api.Models;
using RealEstate.Application.Features.Properties;
using RealEstate.Application.Interfaces;

namespace RealEstate.Api.Controllers;

[ApiController]
[Route("api/properties")]
public sealed class PropertiesController(IPropertyQueryService queryService, IPropertyCommandService commandService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] PropertyFilterRequest request, CancellationToken ct) =>
        this.ToActionResult(await queryService.SearchAsync(request, ct));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct) =>
        this.ToActionResult(await queryService.GetByIdAsync(id, ct));

    [Authorize(Roles = "Vendor,Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromForm] PropertyFormRequest request, CancellationToken ct)
    {
        var result = await commandService.CreateAsync(request.ToApplication(), UserId(), ct);
        if (!result.Success)
            return this.ToActionResult(result);
        return CreatedAtAction(nameof(GetById), new { id = result.Data }, new { id = result.Data });
    }

    [Authorize(Roles = "Vendor,Admin")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromForm] PropertyFormRequest request, CancellationToken ct) =>
        this.ToActionResult(await commandService.UpdateAsync(id, request.ToApplication(), UserId(), User.IsInRole("Admin"), ct));

    [Authorize(Roles = "Vendor,Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct) =>
        this.ToActionResult(await commandService.DeleteAsync(id, UserId(), User.IsInRole("Admin"), ct));

    [Authorize]
    [HttpGet("mine")]
    public async Task<IActionResult> Mine(CancellationToken ct) => Ok(await queryService.GetMineAsync(UserId(), ct));

    [Authorize(Roles = "Admin")]
    [HttpGet("admin/pending")]
    public async Task<IActionResult> Pending(CancellationToken ct) => Ok(await queryService.GetPendingAsync(ct));

    [Authorize(Roles = "Admin")]
    [HttpPatch("admin/{id:int}/status")]
    public async Task<IActionResult> Approve(int id, ApprovePropertyRequest request, CancellationToken ct) =>
        this.ToActionResult(await commandService.ApproveAsync(id, request, ct));

    private string UserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException();
}