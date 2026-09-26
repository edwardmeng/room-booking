namespace RoomBooking.BusinessContracts;

public sealed record CreateReservationResult(
    CreateReservationOutcome Outcome,
    ReservationModel? Reservation);