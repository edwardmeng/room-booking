namespace RoomBooking.Common;

public class DateTimeUtils
{
    public static bool IsFuture(
        DateOnly date,
        TimeOnly start,
        TimeProvider timeProvider)
    {
        var localNow = timeProvider.GetLocalNow();
        var localDate = DateOnly.FromDateTime(localNow.DateTime);
        var localTime = TimeOnly.FromDateTime(localNow.DateTime);

        return date > localDate || (date == localDate && start > localTime);
    }
}
