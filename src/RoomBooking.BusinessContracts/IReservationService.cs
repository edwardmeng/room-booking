namespace RoomBooking.BusinessContracts;

/// <summary>
/// Provides operations for querying and creating room reservations.
/// </summary>
public interface IReservationService
{
    /// <summary>
    /// Lists a room's reservations for a local date in chronological order.
    /// </summary>
    /// <param name="roomId">The room identifier.</param>
    /// <param name="date">The local date to query.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The room existence state and matching reservations.</returns>
    Task<RoomScheduleResult> ListForRoomAsync(
        int roomId,
        DateOnly date,
        CancellationToken cancellationToken);

    /// <summary>
    /// Gets a reservation by its identifier.
    /// </summary>
    /// <param name="id">The reservation identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The reservation when found; otherwise, <see langword="null" />.</returns>
    Task<ReservationModel?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken);

    /// <summary>
    /// Creates a reservation when the room exists and the requested interval is available.
    /// </summary>
    /// <param name="request">The reservation details.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The creation outcome and created reservation, when successful.</returns>
    Task<CreateReservationResult> CreateAsync(
        CreateReservationModel request,
        CancellationToken cancellationToken);
}
