using Microsoft.EntityFrameworkCore;
using RoomBooking.BusinessContracts;
using RoomBooking.Common;
using RoomBooking.DataAccess;
using RoomBooking.DataAccess.Entities;

namespace RoomBooking.BusinessServices;

public sealed class ReservationService : IReservationService
{
    private readonly IRoomService _roomService;
    private readonly RoomBookingDbContext _dbContext;
    private readonly TimeProvider _timeProvider;

    public ReservationService(
        RoomBookingDbContext dbContext,
        TimeProvider timeProvider, IRoomService roomService)
    {
        _dbContext = dbContext;
        _timeProvider = timeProvider;
        _roomService = roomService;
    }

    public async Task<RoomScheduleResult> ListForRoomAsync(
        int roomId,
        DateOnly date,
        CancellationToken cancellationToken)
    {
        var room = await _roomService.GetByIdAsync(roomId, cancellationToken);

        if (room is null)
        {
            return new RoomScheduleResult(false, []);
        }

        var rows = await _dbContext.Reservations
            .AsNoTracking()
            .Where(reservation => reservation.RoomId == roomId && reservation.Date == date)
            .OrderBy(reservation => reservation.StartMinute)
            .ToArrayAsync(cancellationToken);

        return new RoomScheduleResult(true, rows.Select(MapToModel).ToArray());
    }

    public async Task<ReservationModel?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var row = await _dbContext.Reservations
            .AsNoTracking()
            .Where(reservation => reservation.Id == id)
            .SingleOrDefaultAsync(cancellationToken);

        return row is null ? null : MapToModel(row);
    }

    public async Task<CreateReservationResult> CreateAsync(
        CreateReservationModel request,
        CancellationToken cancellationToken)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(request.RoomId);

        if (request.Start >= request.End)
        {
            throw new ArgumentException(
                "Reservation end must be later than its start on the same date.",
                nameof(request));
        }

        if (!DateTimeUtils.IsFuture(request.Date, request.Start, _timeProvider))
        {
            throw new ArgumentException(
                "Reservation start must be in the future.",
                nameof(request));
        }

        var room = await _roomService.GetByIdAsync(request.RoomId, cancellationToken);

        if (room is null)
        {
            return new CreateReservationResult(
                CreateReservationOutcome.RoomNotFound,
                null);
        }

        var requestedStart = request.Start.Hour * 60 + request.Start.Minute;
        var requestedEnd = request.End.Hour * 60 + request.End.Minute;
        var overlaps = await _dbContext.Reservations
            .AsNoTracking()
            .AnyAsync(
                reservation =>
                    reservation.RoomId == request.RoomId &&
                    reservation.Date == request.Date &&
                    reservation.StartMinute < requestedEnd &&
                    reservation.EndMinute > requestedStart,
                cancellationToken);

        if (overlaps)
        {
            return new CreateReservationResult(
                CreateReservationOutcome.TimeConflict,
                null);
        }

        var entity = new ReservationEntity
        {
            RoomId = request.RoomId,
            Title = request.Title,
            Date = request.Date,
            StartMinute = requestedStart,
            EndMinute = requestedEnd,
            CreatedAt = _timeProvider.GetUtcNow().UtcDateTime
        };

        await _dbContext.Reservations.AddAsync(entity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CreateReservationResult(CreateReservationOutcome.Created, MapToModel(entity));
    }

    private static ReservationModel MapToModel(ReservationEntity row) =>
        new(
            row.Id,
            row.Title,
            row.Date,
            new(row.StartMinute / 60, row.StartMinute % 60),
            new(row.EndMinute / 60, row.EndMinute % 60),
            DateTime.SpecifyKind(row.CreatedAt, DateTimeKind.Utc));
}