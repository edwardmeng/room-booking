using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RoomBooking.BusinessContracts;

namespace RoomBooking.IntegrationTests;

public sealed class ErrorHandlingTests
{
    [Fact]
    public async Task UnexpectedFailure_ReturnsSafeInternalErrorProblem()
    {
        await using var factory = new WebApplicationFactory(services =>
        {
            _ = services.RemoveAll<IRoomService>();
            _ = services.AddScoped<IRoomService, ThrowingRoomService>();
        });
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/v1/rooms");
        var payload = await response.Content.ReadAsStringAsync();

        await IntegrationTestSupport.AssertProblemAsync(
            response,
            HttpStatusCode.InternalServerError,
            "INTERNAL_ERROR");
        Assert.DoesNotContain("secret SQL", payload);
        Assert.DoesNotContain(factory.ConnectionString, payload);
    }

    private sealed class ThrowingRoomService : IRoomService
    {
        public Task<RoomModel[]> ListRoomsAsync(CancellationToken cancellationToken) =>
            throw new InvalidOperationException("secret SQL and connection string");

        public Task<RoomModel?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken) =>
            throw new InvalidOperationException("secret SQL and connection string");
    }
}
