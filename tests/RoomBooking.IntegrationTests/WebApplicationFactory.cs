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
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:RoomBooking"] = ConnectionString
            });
        });
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<RoomBookingDbContext>();
            services.RemoveAll<DbContextOptions<RoomBookingDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<RoomBookingDbContext>>();
            services.AddRoomBookingDataAccess(ConnectionString);
            services.RemoveAll<TimeProvider>();
            services.AddSingleton(_timeProvider);
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