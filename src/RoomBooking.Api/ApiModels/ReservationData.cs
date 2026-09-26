namespace RoomBooking.Api.ApiModels;

/// <summary>
/// Represents a reservation returned by the API.
/// </summary>
public sealed record ReservationData(
    int Id,
    string Title,
    string Date,
    string Start,
    string End,
    string CreatedAt);