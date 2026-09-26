namespace RoomBooking.BusinessContracts;

public enum CreateReservationOutcome
{
    Created,
    RoomNotFound,
    TimeConflict
}