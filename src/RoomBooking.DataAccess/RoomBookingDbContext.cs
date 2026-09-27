using Microsoft.EntityFrameworkCore;
using RoomBooking.DataAccess.Entities;

namespace RoomBooking.DataAccess;

/// <summary>
/// Provides access to the room booking database.
/// </summary>
/// <param name="options">The options used to configure the context.</param>
public sealed class RoomBookingDbContext(DbContextOptions<RoomBookingDbContext> options) : DbContext(options)
{
    /// <summary>Gets the rooms data set.</summary>
    public DbSet<RoomEntity> Rooms => Set<RoomEntity>();

    /// <summary>Gets the reservations data set.</summary>
    public DbSet<ReservationEntity> Reservations => Set<ReservationEntity>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder) => modelBuilder.ApplyConfigurationsFromAssembly(typeof(RoomBookingDbContext).Assembly);
}
