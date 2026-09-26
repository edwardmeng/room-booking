namespace RoomBooking.Api.ApiModels;

/// <summary>
/// Represents a room returned by the API.
/// </summary>
public sealed record RoomData(
    int Id,
    string Name,
    string Location,
    string? Description,
    int Capacity);