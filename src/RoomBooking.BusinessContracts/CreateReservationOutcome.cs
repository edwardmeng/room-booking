namespace RoomBooking.BusinessContracts;

/// <summary>
/// Identifies the outcome of a reservation creation attempt.
/// </summary>
public enum CreateReservationOutcome
{
    /// <summary>The reservation was created.</summary>
    Created,

    /// <summary>The requested room does not exist.</summary>
    RoomNotFound,

    /// <summary>The requested interval overlaps an existing reservation.</summary>
    TimeConflict
}
