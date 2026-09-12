namespace RealEstate.Domain.Entities;

public enum ViewingAppointmentStatus
{
    Pending,
    Confirmed,
    Rejected,
    Cancelled,
    Completed,
    NoShow
}

public sealed class ViewingAppointment
{
    public int Id { get; set; }
    public int PropertyId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public string? Notes { get; set; }
    public ViewingAppointmentStatus Status { get; set; } = ViewingAppointmentStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Property Property { get; set; } = null!;
    public User User { get; set; } = null!;
}
