namespace RoomBooking.DataAccess.Entities;

/// <summary>
/// Represents a persisted room reservation.
/// </summary>
public sealed class ReservationEntity
{
    /// <summary>Gets or sets the reservation identifier.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the reserved room identifier.</summary>
    public int RoomId { get; set; }

    /// <summary>Gets or sets the reservation title.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Gets or sets the local reservation date.</summary>
    public DateOnly Date { get; set; }

    /// <summary>Gets or sets the start minute relative to midnight.</summary>
    public int StartMinute { get; set; }

    /// <summary>Gets or sets the end minute relative to midnight.</summary>
    public int EndMinute { get; set; }

    /// <summary>Gets or sets the UTC creation timestamp.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Gets or sets the reserved room.</summary>
    public RoomEntity Room { get; set; } = null!;
}
