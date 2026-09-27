namespace RoomBooking.Common;

/// <summary>
/// Provides date and time comparison helpers.
/// </summary>
public class DateTimeUtils
{
    /// <summary>
    /// Determines whether a local date and start time occur after the current local time.
    /// </summary>
    /// <param name="date">The local calendar date.</param>
    /// <param name="start">The local start time.</param>
    /// <param name="timeProvider">The source of the current time and local time zone.</param>
    /// <returns><see langword="true" /> when the supplied date and time are in the future; otherwise, <see langword="false" />.</returns>
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
