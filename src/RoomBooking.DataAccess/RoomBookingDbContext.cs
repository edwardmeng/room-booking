using Microsoft.EntityFrameworkCore;
using RoomBooking.DataAccess.Entities;

namespace RoomBooking.DataAccess;

public sealed class RoomBookingDbContext : DbContext
{
    public RoomBookingDbContext(DbContextOptions<RoomBookingDbContext> options)
        : base(options)
    {
    }

    public DbSet<RoomEntity> Rooms => Set<RoomEntity>();

    public DbSet<ReservationEntity> Reservations => Set<ReservationEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(RoomBookingDbContext).Assembly);
    }
}