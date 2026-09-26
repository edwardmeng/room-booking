using Microsoft.EntityFrameworkCore;
using RoomBooking.BusinessContracts;
using RoomBooking.DataAccess;
using RoomBooking.DataAccess.Entities;

namespace RoomBooking.BusinessServices;

public sealed class RoomService : IRoomService
{
    private readonly RoomBookingDbContext _dbContext;

    public RoomService(RoomBookingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<RoomModel[]> ListRoomsAsync(CancellationToken cancellationToken)
    {
        var entities = await _dbContext.Rooms
            .AsNoTracking()
            .OrderBy(room => room.Name)
            .ToArrayAsync(cancellationToken);
        return entities.Select(MapToModel).ToArray();
    }

    public async Task<RoomModel?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Rooms.AsNoTracking()
            .Where(room => room.Id == id)
            .SingleOrDefaultAsync(cancellationToken);
        return entity != null ? MapToModel(entity) : null;
    }

    private RoomModel MapToModel(RoomEntity entity)
        => new(
            entity.Id,
            entity.Name,
            entity.Location,
            entity.Description,
            entity.Capacity);
}