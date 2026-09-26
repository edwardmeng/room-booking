namespace RoomBooking.BusinessContracts;

public sealed record CreateReservationModel(
    int RoomId,
    string Title,
    DateOnly Date,
    TimeOnly Start,
    TimeOnly End);
