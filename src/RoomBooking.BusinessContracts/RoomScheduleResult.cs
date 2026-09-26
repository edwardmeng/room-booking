namespace RoomBooking.BusinessContracts;

public sealed record RoomScheduleResult(
    bool RoomExists,
    ReservationModel[] Reservations);
