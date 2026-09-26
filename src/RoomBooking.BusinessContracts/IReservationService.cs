namespace RoomBooking.BusinessContracts;

public interface IReservationService
{
    Task<RoomScheduleResult> ListForRoomAsync(
        int roomId,
        DateOnly date,
        CancellationToken cancellationToken);

    Task<ReservationModel?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken);

    Task<CreateReservationResult> CreateAsync(
        CreateReservationModel request,
        CancellationToken cancellationToken);
}