namespace RoomBooking.UnitTests;

internal sealed class TestTimeProvider(
    DateTimeOffset utcNow,
    TimeZoneInfo localTimeZone) : TimeProvider
{
    public override TimeZoneInfo LocalTimeZone { get; } = localTimeZone;

    public override DateTimeOffset GetUtcNow() => utcNow.ToUniversalTime();
}
