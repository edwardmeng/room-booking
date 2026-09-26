namespace RoomBooking.BusinessContracts;

public interface IRoomService
{
    Task<RoomModel[]> ListRoomsAsync(CancellationToken cancellationToken);

    Task<RoomModel?> GetByIdAsync(int id, CancellationToken cancellationToken);
}
