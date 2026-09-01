using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RealEstate.Application.Common;
using RealEstate.Application.Features.Auth;
using RealEstate.Application.Interfaces;
using RealEstate.Domain.Entities;
using RealEstate.Domain.Enums;
using RealEstate.Infrastructure.Persistence;

namespace RealEstate.Infrastructure.Services;

public sealed class AuthService(AppDbContext db, ITokenService tokenService) : IAuthService
{
    private readonly PasswordHasher<User> _passwordHasher = new();

    public async Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken ct)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (request.Role == UserRole.Admin) return Result<AuthResponse>.Fail(ErrorCode.Forbidden, "Admin registration is not allowed.");
        if (await db.Users.AnyAsync(x => x.Email == email, ct)) return Result<AuthResponse>.Fail(ErrorCode.Conflict, "Email already exists.");

        var user = new User
        {
            FirstName = request.FirstName.Trim(), LastName = request.LastName.Trim(), Email = email,
            PhoneNumber = request.PhoneNumber?.Trim(), Role = request.Role
        };
        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);
        db.Users.Add(user);
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateException) { return Result<AuthResponse>.Fail(ErrorCode.Conflict, "The account could not be created because the email already exists."); }
        return Result<AuthResponse>.Ok(ToResponse(user));
    }

    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken ct)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await db.Users.SingleOrDefaultAsync(x => x.Email == email, ct);
        if (user is null) return Result<AuthResponse>.Fail(ErrorCode.Unauthorized, "Invalid email or password.");
        var verification = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (verification == PasswordVerificationResult.Failed) return Result<AuthResponse>.Fail(ErrorCode.Unauthorized, "Invalid email or password.");
        return Result<AuthResponse>.Ok(ToResponse(user));
    }

    private AuthResponse ToResponse(User user) => new(tokenService.CreateToken(user), user.Id, user.FirstName, user.LastName, user.Email, user.Role);
}
