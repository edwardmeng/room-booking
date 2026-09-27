namespace RoomBooking.BusinessContracts;

/// <summary>
/// Represents a room schedule query result.
/// </summary>
/// <param name="RoomExists">Indicates whether the requested room exists.</param>
/// <param name="Reservations">The reservations scheduled for the requested date.</param>
public sealed record RoomScheduleResult(
    bool RoomExists,
    ReservationModel[] Reservations);
