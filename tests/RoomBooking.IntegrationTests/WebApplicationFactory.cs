using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RoomBooking.DataAccess;

namespace RoomBooking.IntegrationTests;

internal sealed class WebApplicationFactory(Action<IServiceCollection>? configureServices = null) : WebApplicationFactory<Program>
{
    private readonly string _databasePath = Path.Combine(Path.GetTempPath(), $"room-booking-tests-{Guid.NewGuid():N}.db");
    private readonly TimeProvider _timeProvider = new FixedTimeProvider(
        new DateTimeOffset(2026, 10, 5, 2, 0, 0, TimeSpan.Zero));

    public string ConnectionString => $"Data Source={_databasePath};Foreign Keys=True;Pooling=False";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        _ = builder.UseEnvironment("Testing");
        _ = builder.ConfigureAppConfiguration((context, configuration) =>
        {
            _ = configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:RoomBooking"] = ConnectionString
            });
        });
        _ = builder.ConfigureServices(services =>
        {
            _ = services.RemoveAll<RoomBookingDbContext>();
            _ = services.RemoveAll<DbContextOptions<RoomBookingDbContext>>();
            _ = services.RemoveAll<IDbContextOptionsConfiguration<RoomBookingDbContext>>();
            _ = services.AddRoomBookingDataAccess(ConnectionString);
            _ = services.RemoveAll<TimeProvider>();
            _ = services.AddSingleton(_timeProvider);
            configureServices?.Invoke(services);
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        DeleteDatabaseFile(_databasePath);
        DeleteDatabaseFile($"{_databasePath}-wal");
        DeleteDatabaseFile($"{_databasePath}-shm");
    }

    private static void DeleteDatabaseFile(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;

        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}
