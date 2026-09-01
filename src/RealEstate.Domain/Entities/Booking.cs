using RealEstate.Domain.Enums;

namespace RealEstate.Domain.Entities;

public sealed class Booking
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public User User { get; set; } = null!;
    public int PropertyId { get; set; }
    public Property Property { get; set; } = null!;
    public DateTime BookingDate { get; set; } = DateTime.UtcNow;
    public BookingStatus Status { get; set; } = BookingStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
