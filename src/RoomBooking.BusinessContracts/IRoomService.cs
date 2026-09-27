namespace RoomBooking.BusinessContracts;

/// <summary>
/// Provides operations for querying rooms.
/// </summary>
public interface IRoomService
{
    /// <summary>
    /// Lists all rooms in display order.
    /// </summary>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The available rooms.</returns>
    Task<RoomModel[]> ListRoomsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Gets a room by its identifier.
    /// </summary>
    /// <param name="id">The room identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The room when found; otherwise, <see langword="null" />.</returns>
    Task<RoomModel?> GetByIdAsync(int id, CancellationToken cancellationToken);
}
