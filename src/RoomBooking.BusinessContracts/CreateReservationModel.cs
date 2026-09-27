namespace RoomBooking.BusinessContracts;

/// <summary>
/// Describes a request to reserve a room for a time interval.
/// </summary>
/// <param name="RoomId">The identifier of the room to reserve.</param>
/// <param name="Title">The reservation title.</param>
/// <param name="Date">The local reservation date.</param>
/// <param name="Start">The local reservation start time.</param>
/// <param name="End">The local reservation end time.</param>
public sealed record CreateReservationModel(
    int RoomId,
    string Title,
    DateOnly Date,
    TimeOnly Start,
    TimeOnly End);
