namespace RoomBooking.DataAccess.Entities;

public sealed class RoomEntity
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int Capacity { get; set; }

    public ICollection<ReservationEntity> Reservations { get; } = [];
}