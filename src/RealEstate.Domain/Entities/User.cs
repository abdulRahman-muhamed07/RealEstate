using System.ComponentModel.DataAnnotations;
using RealEstate.Domain.Enums;

namespace RealEstate.Domain.Entities;

public sealed class User
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    [MaxLength(100)] public string FirstName { get; set; } = string.Empty;
    [MaxLength(100)] public string LastName { get; set; } = string.Empty;
    [MaxLength(256)] public string Email { get; set; } = string.Empty;
    [MaxLength(30)] public string? PhoneNumber { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.User;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    [MaxLength(150)] public string? CompanyName { get; set; }
    [MaxLength(1000)] public string? Bio { get; set; }
    public bool IsVerifiedVendor { get; set; }
    public ICollection<Property> Properties { get; set; } = new List<Property>();
    public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public ICollection<SavedSearch> SavedSearches { get; set; } = new List<SavedSearch>();
    public ICollection<ViewingAppointment> ViewingAppointments { get; set; } = new List<ViewingAppointment>();
}
