using System.Net;
using RoomBooking.Common;

namespace RoomBooking.IntegrationTests;

public sealed class RoomEndpointTests
{
    [Fact]
    public async Task ListRooms_ReturnsCompleteStableCatalog()
    {
        await using var factory = new WebApplicationFactory();
        using var client = factory.CreateClient();
        _ = await IntegrationTestSupport.CreateReservationAsync(
            client,
            1,
            "Existing reservation",
            new DateOnly(2030, 1, 16),
            new TimeOnly(10, 0),
            new TimeOnly(11, 0));

        var response = await client.GetAsync("/api/v1/rooms");
        using var document = await response.ReadAsJsonAsync();
        Assert.All(
            document.RootElement.EnumerateArray(),
            room => Assert.False(room.TryGetProperty("reservations", out _)));
        var rooms = document.RootElement.EnumerateArray().ToArray();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(3, rooms.Length);
        Assert.Equal(
            new[] { "Auditorium", "Conference Room", "Small Room" },
            rooms.Select(room => room.GetProperty("name").GetString()).ToArray());
        Assert.Equal([50, 10, 4], [.. rooms.Select(room => room.GetProperty("capacity").GetInt32())]);
        Assert.All(rooms, room =>
        {
            Assert.True(room.TryGetProperty("id", out _));
            Assert.True(room.TryGetProperty("location", out _));
            Assert.True(room.TryGetProperty("description", out _));
        });
    }
}
