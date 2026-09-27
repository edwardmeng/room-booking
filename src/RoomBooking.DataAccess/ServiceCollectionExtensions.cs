using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace RoomBooking.DataAccess;

/// <summary>
/// Provides dependency injection registration for room booking data access.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the room booking database context backed by SQLite.
    /// </summary>
    /// <param name="services">The service collection to update.</param>
    /// <param name="connectionString">The SQLite connection string.</param>
    /// <returns>The supplied service collection.</returns>
    public static IServiceCollection AddRoomBookingDataAccess(
        this IServiceCollection services,
        string connectionString)
    {
        _ = services.AddDbContext<RoomBookingDbContext>(options =>
            options.UseSqlite(connectionString));

        return services;
    }
}
