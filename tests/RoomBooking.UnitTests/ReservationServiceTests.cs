using Microsoft.EntityFrameworkCore;
using RoomBooking.BusinessContracts;
using RoomBooking.DataAccess.Entities;

namespace RoomBooking.UnitTests;

public sealed class ReservationServiceTests
{
    private static readonly DateTimeOffset UtcNow = new(2026, 10, 5, 2, 0, 0, TimeSpan.Zero);

    private static readonly DateOnly FutureDate = new(2026, 10, 6);

    [Fact]
    public async Task ListForRoomAsync_UnknownRoom()
    {
        await using var host = await CreateHostAsync();
        var service = host.GetRequiredService<IReservationService>();

        var result = await service.ListForRoomAsync(999, FutureDate, CancellationToken.None);

        Assert.False(result.RoomExists);
        Assert.Empty(result.Reservations);
    }

    [Fact]
    public async Task ListForRoomAsync_ExistingRoomWithoutReservations()
    {
        await using var host = await CreateHostAsync();
        var service = host.GetRequiredService<IReservationService>();

        var result = await service.ListForRoomAsync(1, FutureDate, CancellationToken.None);

        Assert.True(result.RoomExists);
        Assert.Empty(result.Reservations);
    }

    [Fact]
    public async Task ListForRoomAsync_FiltersOrdersAndMapsReservations()
    {
        await using var host = await CreateHostAsync();
        host.DbContext.Reservations.AddRange(
            CreateReservationEntity(1, "Later", FutureDate, 660, 720),
            CreateReservationEntity(1, "Earlier", FutureDate, 540, 600),
            CreateReservationEntity(2, "Other room", FutureDate, 480, 540),
            CreateReservationEntity(1, "Other date", FutureDate.AddDays(1), 480, 540));
        await host.DbContext.SaveChangesAsync();
        var service = host.GetRequiredService<IReservationService>();

        var result = await service.ListForRoomAsync(1, FutureDate, CancellationToken.None);

        Assert.True(result.RoomExists);
        Assert.Equal(["Earlier", "Later"], result.Reservations.Select(item => item.Title));
        Assert.Equal(new TimeOnly(9, 0), result.Reservations[0].Start);
        Assert.Equal(new TimeOnly(10, 0), result.Reservations[0].End);
        Assert.All(result.Reservations, item => Assert.Equal(DateTimeKind.Utc, item.CreatedAt.Kind));
    }

    [Fact]
    public async Task GetByIdAsync_ExistingReservation()
    {
        await using var host = await CreateHostAsync();
        var entity = CreateReservationEntity(1, "Architecture review", FutureDate, 485, 555);
        host.DbContext.Reservations.Add(entity);
        await host.DbContext.SaveChangesAsync();
        host.DbContext.ChangeTracker.Clear();
        var service = host.GetRequiredService<IReservationService>();

        var reservation = await service.GetByIdAsync(entity.Id, CancellationToken.None);

        Assert.NotNull(reservation);
        Assert.Equal(entity.Id, reservation.Id);
        Assert.Equal("Architecture review", reservation.Title);
        Assert.Equal(FutureDate, reservation.Date);
        Assert.Equal(new TimeOnly(8, 5), reservation.Start);
        Assert.Equal(new TimeOnly(9, 15), reservation.End);
        Assert.Equal(UtcNow.UtcDateTime, reservation.CreatedAt);
        Assert.Equal(DateTimeKind.Utc, reservation.CreatedAt.Kind);
        Assert.Empty(host.DbContext.ChangeTracker.Entries());
    }

    [Fact]
    public async Task GetByIdAsync_UnknownReservation()
    {
        await using var host = await CreateHostAsync();
        var service = host.GetRequiredService<IReservationService>();

        var reservation = await service.GetByIdAsync(999, CancellationToken.None);

        Assert.Null(reservation);
    }

    [Fact]
    public async Task CreateAsync_NonPositiveRoomId()
    {
        await using var host = await CreateHostAsync();
        var service = host.GetRequiredService<IReservationService>();

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            service.CreateAsync(CreateReservationRequest(roomId: 0), CancellationToken.None));

        Assert.Equal(0, await host.DbContext.Reservations.CountAsync());
    }

    [Theory]
    [InlineData(10, 0, 10, 0)]
    [InlineData(11, 0, 10, 0)]
    public async Task CreateAsync_UnorderedTimes(int startHour, int startMinute, int endHour, int endMinute)
    {
        await using var host = await CreateHostAsync();
        var service = host.GetRequiredService<IReservationService>();
        var request = CreateReservationRequest(
            start: new TimeOnly(startHour, startMinute),
            end: new TimeOnly(endHour, endMinute));

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.CreateAsync(request, CancellationToken.None));

        Assert.Equal(0, await host.DbContext.Reservations.CountAsync());
    }

    [Theory]
    [InlineData(1, 59)]
    [InlineData(2, 0)]
    public async Task CreateAsync_NonFutureStart(int startHour, int startMinute)
    {
        await using var host = await CreateHostAsync();
        var service = host.GetRequiredService<IReservationService>();
        var request = CreateReservationRequest(
            date: new DateOnly(2026, 10, 5),
            start: new TimeOnly(startHour, startMinute),
            end: new TimeOnly(3, 0));

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.CreateAsync(request, CancellationToken.None));

        Assert.Equal(0, await host.DbContext.Reservations.CountAsync());
    }

    [Fact]
    public async Task CreateAsync_UnknownRoom()
    {
        await using var host = await CreateHostAsync();
        var service = host.GetRequiredService<IReservationService>();

        var result = await service.CreateAsync(CreateReservationRequest(roomId: 999), CancellationToken.None);

        Assert.Equal(CreateReservationOutcome.RoomNotFound, result.Outcome);
        Assert.Null(result.Reservation);
        Assert.Equal(0, await host.DbContext.Reservations.CountAsync());
    }

    [Theory]
    [InlineData(600, 660)]
    [InlineData(540, 720)]
    [InlineData(615, 645)]
    [InlineData(570, 630)]
    [InlineData(630, 690)]
    public async Task CreateAsync_OverlappingSlot(
        int requestedStart,
        int requestedEnd)
    {
        await using var host = await CreateHostAsync();
        host.DbContext.Reservations.Add(CreateReservationEntity(1, "Existing", FutureDate, 600, 660));
        await host.DbContext.SaveChangesAsync();
        host.DbContext.ChangeTracker.Clear();
        var service = host.GetRequiredService<IReservationService>();
        var request = CreateReservationRequest(
            start: new(requestedStart / 60, requestedStart % 60),
            end: new(requestedEnd / 60, requestedEnd % 60));

        var result = await service.CreateAsync(request, CancellationToken.None);

        Assert.Equal(CreateReservationOutcome.TimeConflict, result.Outcome);
        Assert.Null(result.Reservation);
        Assert.Equal(1, await host.DbContext.Reservations.CountAsync());
    }

    [Theory]
    [InlineData(540, 600)]
    [InlineData(660, 720)]
    public async Task CreateAsync_AdjacentSlot(
        int requestedStart,
        int requestedEnd)
    {
        await using var host = await CreateHostAsync();
        host.DbContext.Reservations.Add(CreateReservationEntity(1, "Existing", FutureDate, 600, 660));
        await host.DbContext.SaveChangesAsync();
        host.DbContext.ChangeTracker.Clear();
        var service = host.GetRequiredService<IReservationService>();
        var request = CreateReservationRequest(
            title: "Adjacent planning",
            start: new(requestedStart / 60, requestedStart % 60),
            end: new(requestedEnd / 60, requestedEnd % 60));

        var result = await service.CreateAsync(request, CancellationToken.None);

        Assert.Equal(CreateReservationOutcome.Created, result.Outcome);
        Assert.NotNull(result.Reservation);
        Assert.True(result.Reservation.Id > 0);
        Assert.Equal("Adjacent planning", result.Reservation.Title);
        Assert.Equal(UtcNow.UtcDateTime, result.Reservation.CreatedAt);
        var persisted = await host.DbContext.Reservations
            .SingleAsync(item => item.Id == result.Reservation.Id);
        Assert.Equal(requestedStart, persisted.StartMinute);
        Assert.Equal(requestedEnd, persisted.EndMinute);
        Assert.Equal(UtcNow.UtcDateTime, persisted.CreatedAt);
        Assert.Equal(DateTimeKind.Utc, result.Reservation.CreatedAt.Kind);
    }
    
    private static Task<ServiceTestHost> CreateHostAsync() =>
        ServiceTestHost.CreateAsync(new TestTimeProvider(UtcNow, TimeZoneInfo.Utc));

    private static CreateReservationModel CreateReservationRequest(
        int roomId = 1,
        string title = "Planning",
        DateOnly? date = null,
        TimeOnly? start = null,
        TimeOnly? end = null) =>
        new(
            roomId,
            title,
            date ?? FutureDate,
            start ?? new TimeOnly(10, 0),
            end ?? new TimeOnly(11, 0));

    private static ReservationEntity CreateReservationEntity(
        int roomId,
        string title,
        DateOnly date,
        int startMinute,
        int endMinute) =>
        new()
        {
            RoomId = roomId,
            Title = title,
            Date = date,
            StartMinute = startMinute,
            EndMinute = endMinute,
            CreatedAt = UtcNow.UtcDateTime
        };
}