namespace RoomBooking.BusinessContracts;

public sealed record ReservationModel(
    int Id,
    string Title,
    DateOnly Date,
    TimeOnly Start,
    TimeOnly End,
    DateTime CreatedAt);