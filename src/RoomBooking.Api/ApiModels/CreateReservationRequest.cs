namespace RoomBooking.Api.ApiModels;

/// <summary>
/// Represents the payload for creating a reservation.
/// </summary>
public sealed record CreateReservationRequest(
    int Room,
    string Title,
    string Date,
    string Start,
    string End);