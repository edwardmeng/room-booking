using RoomBooking.Common;

namespace RoomBooking.UnitTests
{
    public class DateTimeUtilsTests
    {
        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(1)]
        public void IsFuture_Validate(int minuteOffset)
        {
            var now = new DateTimeOffset(2026, 10, 5, 10, 0, 0, TimeSpan.FromHours(8));
            var timeProvider = new TestTimeProvider(now, TimeZoneInfo.CreateCustomTimeZone(
                "Test zone",
                TimeSpan.FromHours(8),
                "Test zone",
                "Test zone"));
            var candidate = now.AddMinutes(minuteOffset);

            var isFuture = DateTimeUtils.IsFuture(
                DateOnly.FromDateTime(candidate.DateTime),
                TimeOnly.FromDateTime(candidate.DateTime),
                timeProvider);

            Assert.Equal(minuteOffset > 0, isFuture);
        }
    }
}
