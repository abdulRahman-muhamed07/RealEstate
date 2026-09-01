using Microsoft.AspNetCore.Mvc;
using RealEstate.Application.Features.Auth;
using RealEstate.Application.Interfaces;
using RealEstate.Api.Extensions;

namespace RealEstate.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IAuthService service) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken ct) =>
        (await service.RegisterAsync(request, ct)).ToActionResult(this);

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken ct) =>
        (await service.LoginAsync(request, ct)).ToActionResult(this);
}
