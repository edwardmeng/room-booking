namespace RoomBooking.BusinessContracts;

/// <summary>
/// Represents a room reservation.
/// </summary>
/// <param name="Id">The reservation identifier.</param>
/// <param name="Title">The reservation title.</param>
/// <param name="Date">The local reservation date.</param>
/// <param name="Start">The local start time.</param>
/// <param name="End">The local end time.</param>
/// <param name="CreatedAt">The UTC timestamp at which the reservation was created.</param>
public sealed record ReservationModel(
    int Id,
    string Title,
    DateOnly Date,
    TimeOnly Start,
    TimeOnly End,
    DateTime CreatedAt);
