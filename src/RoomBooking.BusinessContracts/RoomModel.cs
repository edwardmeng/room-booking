namespace RoomBooking.BusinessContracts;

public sealed record RoomModel(
    int Id,
    string Name,
    string Location,
    string? Description,
    int Capacity);
