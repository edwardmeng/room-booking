namespace RoomBooking.DataAccess.Entities;

/// <summary>
/// Represents a persisted room available for reservation.
/// </summary>
public sealed class RoomEntity
{
    /// <summary>Gets or sets the room identifier.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the room name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the room location.</summary>
    public string Location { get; set; } = string.Empty;

    /// <summary>Gets or sets the optional room description.</summary>
    public string? Description { get; set; }

    /// <summary>Gets or sets the maximum number of occupants.</summary>
    public int Capacity { get; set; }

    /// <summary>Gets the reservations associated with the room.</summary>
    public ICollection<ReservationEntity> Reservations { get; } = [];
}
