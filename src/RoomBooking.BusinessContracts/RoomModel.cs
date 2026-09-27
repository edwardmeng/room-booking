namespace RoomBooking.BusinessContracts;

/// <summary>
/// Represents a room available for reservation.
/// </summary>
/// <param name="Id">The room identifier.</param>
/// <param name="Name">The room name.</param>
/// <param name="Location">The room location.</param>
/// <param name="Description">The optional room description.</param>
/// <param name="Capacity">The maximum number of occupants.</param>
public sealed record RoomModel(
    int Id,
    string Name,
    string Location,
    string? Description,
    int Capacity);
