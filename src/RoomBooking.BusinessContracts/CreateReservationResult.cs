namespace RoomBooking.BusinessContracts;

/// <summary>
/// Represents the result of a reservation creation attempt.
/// </summary>
/// <param name="Outcome">The creation outcome.</param>
/// <param name="Reservation">The created reservation, or <see langword="null" /> when creation did not succeed.</param>
public sealed record CreateReservationResult(
    CreateReservationOutcome Outcome,
    ReservationModel? Reservation);
