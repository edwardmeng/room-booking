using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RoomBooking.BusinessContracts;
using RoomBooking.BusinessServices;
using RoomBooking.DataAccess;

namespace RoomBooking.UnitTests;

internal sealed class ServiceTestHost : IAsyncDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ServiceProvider _serviceProvider;
    private readonly AsyncServiceScope _scope;

    private ServiceTestHost(
        SqliteConnection connection,
        ServiceProvider serviceProvider,
        AsyncServiceScope scope)
    {
        _connection = connection;
        _serviceProvider = serviceProvider;
        _scope = scope;
    }

    public IServiceProvider Services => _scope.ServiceProvider;

    public RoomBookingDbContext DbContext =>
        Services.GetRequiredService<RoomBookingDbContext>();

    public static async Task<ServiceTestHost> CreateAsync(
        TimeProvider? timeProvider = null)
    {
        var connection = new SqliteConnection("Data Source=:memory:;Foreign Keys=True");
        await connection.OpenAsync();

        var services = new ServiceCollection();
        _ = services.AddSingleton(connection);
        _ = services.AddDbContext<RoomBookingDbContext>((provider, options) =>
            options.UseSqlite(provider.GetRequiredService<SqliteConnection>()));
        _ = services.AddSingleton(timeProvider ?? TimeProvider.System);
        _ = services.AddScoped<IRoomService, RoomService>();
        _ = services.AddScoped<IReservationService, ReservationService>();

        var serviceProvider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true
        });
        var scope = serviceProvider.CreateAsyncScope();
        var host = new ServiceTestHost(connection, serviceProvider, scope);
        _ = await host.DbContext.Database.EnsureCreatedAsync();
        return host;
    }

    public TService GetRequiredService<TService>() where TService : notnull =>
        Services.GetRequiredService<TService>();

    public async ValueTask DisposeAsync()
    {
        await _scope.DisposeAsync();
        await _serviceProvider.DisposeAsync();
        await _connection.DisposeAsync();
    }
}
