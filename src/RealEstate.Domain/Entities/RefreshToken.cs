namespace RealEstate.Domain.Entities;

public sealed class RefreshToken
{
    public int Id { get; set; }
    public string Token { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RevokedAt { get; set; }
    public User User { get; set; } = null!;
    public bool IsActive => RevokedAt is null && ExpiresAt > DateTime.UtcNow;
}
