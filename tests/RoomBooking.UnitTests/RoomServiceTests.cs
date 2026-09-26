using RoomBooking.BusinessContracts;
using RoomBooking.DataAccess.Entities;

namespace RoomBooking.UnitTests;

public sealed class RoomServiceTests
{
    [Fact]
    public async Task ListRoomsAsync_ReturnsAllRoomsOrderedByName()
    {
        await using var host = await ServiceTestHost.CreateAsync();
        _ = host.DbContext.Rooms.Add(new RoomEntity
        {
            Name = "Alpha room",
            Location = "North wing",
            Description = "Interview room",
            Capacity = 6
        });
        _ = await host.DbContext.SaveChangesAsync();
        host.DbContext.ChangeTracker.Clear();
        var service = host.GetRequiredService<IRoomService>();

        var rooms = await service.ListRoomsAsync(CancellationToken.None);

        Assert.Equal(4, rooms.Length);
        Assert.Equal(
            ["Alpha room", "Auditorium", "Conference Room", "Small Room"],
            rooms.Select(room => room.Name));
        Assert.Equal("Interview room", rooms[0].Description);
        Assert.Equal("North wing", rooms[0].Location);
        Assert.Equal(6, rooms[0].Capacity);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingRoom_ReturnsCompleteModel()
    {
        await using var host = await ServiceTestHost.CreateAsync();
        var service = host.GetRequiredService<IRoomService>();

        var room = await service.GetByIdAsync(2, CancellationToken.None);

        Assert.NotNull(room);
        Assert.Equal(2, room.Id);
        Assert.Equal("Conference Room", room.Name);
        Assert.Equal("Floor 2, Central Wing", room.Location);
        Assert.Equal("Conference room with presentation facilities.", room.Description);
        Assert.Equal(10, room.Capacity);
    }

    [Fact]
    public async Task GetByIdAsync_UnknownRoom_ReturnsNull()
    {
        await using var host = await ServiceTestHost.CreateAsync();
        var service = host.GetRequiredService<IRoomService>();

        var room = await service.GetByIdAsync(999, CancellationToken.None);

        Assert.Null(room);
    }
}
