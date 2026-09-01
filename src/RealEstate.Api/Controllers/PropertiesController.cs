using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealEstate.Api.Extensions;
using RealEstate.Api.Models;
using RealEstate.Application.Features.Properties;
using RealEstate.Application.Interfaces;
using RealEstate.Domain.Entities;

namespace RealEstate.Api.Controllers;

[ApiController]
[Route("api/properties")]
public sealed class PropertiesController(IPropertyService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] PropertyFilterRequest request, CancellationToken ct) => Ok(await service.SearchAsync(request, ct).ContinueWith(t => t.Result.Data, ct));

    [HttpGet("search")]
    public async Task<IActionResult> SearchExplicit([FromQuery] PropertyFilterRequest request, CancellationToken ct)
    {
        var result = await service.SearchAsync(request, ct);
        return result.Success ? Ok(result.Data) : this.ToActionResult(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct) => this.ToActionResult(await service.GetByIdAsync(id, ct));

    [Authorize(Roles = "Vendor,Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromForm] PropertyFormRequest request, CancellationToken ct)
    {
        var result = await service.CreateAsync(request.ToApplication(), UserId(), ct);
        return result.Success ? CreatedAtAction(nameof(GetById), new { id = result.Data }, new { id = result.Data }) : this.ToActionResult(result);
    }

    [Authorize(Roles = "Vendor,Admin")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromForm] PropertyFormRequest request, CancellationToken ct) =>
        this.ToActionResult(await service.UpdateAsync(id, request.ToApplication(), UserId(), User.IsInRole(nameof(UserRole.Admin)), ct));

    [Authorize(Roles = "Vendor,Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct) =>
        this.ToActionResult(await service.DeleteAsync(id, UserId(), User.IsInRole(nameof(UserRole.Admin)), ct));

    [Authorize]
    [HttpGet("mine")]
    public async Task<IActionResult> Mine(CancellationToken ct) => Ok(await service.GetMineAsync(UserId(), ct));

    [Authorize(Roles = "Admin")]
    [HttpGet("admin/pending")]
    public async Task<IActionResult> Pending(CancellationToken ct) => Ok(await service.GetPendingAsync(ct));

    [Authorize(Roles = "Admin")]
    [HttpPatch("admin/{id:int}/status")]
    public async Task<IActionResult> Approve(int id, ApprovePropertyRequest request, CancellationToken ct) =>
        this.ToActionResult(await service.ApproveAsync(id, request, ct));

    private string UserId() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException();
}
