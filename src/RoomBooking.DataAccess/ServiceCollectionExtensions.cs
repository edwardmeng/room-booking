using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace RoomBooking.DataAccess;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRoomBookingDataAccess(
        this IServiceCollection services,
        string connectionString)
    {
        _ = services.AddDbContext<RoomBookingDbContext>(options =>
            options.UseSqlite(connectionString));

        return services;
    }
}
