using System.ComponentModel.DataAnnotations;
using RealEstate.Domain.Entities;

namespace RealEstate.Application.Features.Auth;

public sealed record RegisterRequest(
    [property:Required, StringLength(100)] string FirstName,
    [property:Required, StringLength(100)] string LastName,
    [property:Required, EmailAddress, StringLength(256)] string Email,
    [property:Required, MinLength(8), StringLength(128)] string Password,
    [property:StringLength(30)] string? PhoneNumber,
    UserRole Role = UserRole.User);

public sealed record LoginRequest(
    [property:Required, EmailAddress, StringLength(256)] string Email,
    [property:Required, StringLength(128)] string Password);

public sealed record AuthResponse(string Token, string UserId, string FirstName, string LastName, string Email, UserRole Role);
