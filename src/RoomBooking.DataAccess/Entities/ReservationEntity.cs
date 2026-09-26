namespace RoomBooking.DataAccess.Entities;

public sealed class ReservationEntity
{
    public int Id { get; set; }

    public int RoomId { get; set; }

    public string Title { get; set; } = string.Empty;

    public DateOnly Date { get; set; }

    public int StartMinute { get; set; }

    public int EndMinute { get; set; }

    public DateTime CreatedAt { get; set; }

    public RoomEntity Room { get; set; } = null!;
}