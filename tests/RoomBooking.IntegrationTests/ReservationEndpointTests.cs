using System.Net;
using System.Text;
using System.Text.Json;
using RoomBooking.Common;

namespace RoomBooking.IntegrationTests;

public sealed class ReservationEndpointTests
{
    private static readonly DateOnly FutureDate = new(2026, 10, 6);
    private static readonly DateOnly ScheduleDate = new(2030, 1, 16);

    [Fact]
    public async Task RoomAndDate_ReturnOrderedSchedule()
    {
        await using var factory = new WebApplicationFactory();
        using var client = factory.CreateClient();
        var third = await IntegrationTestSupport.CreateReservationAsync(client, 1, "Third", ScheduleDate, new TimeOnly(11, 0), new TimeOnly(12, 0));
        var first = await IntegrationTestSupport.CreateReservationAsync(client, 1, "First", ScheduleDate, new TimeOnly(9, 0), new TimeOnly(10, 0));
        var second = await IntegrationTestSupport.CreateReservationAsync(client, 1, "Second", ScheduleDate, new TimeOnly(10, 0), new TimeOnly(11, 0));
        _ = await IntegrationTestSupport.CreateReservationAsync(client, 2, "Other room", ScheduleDate, new TimeOnly(8, 0), new TimeOnly(9, 0));
        _ = await IntegrationTestSupport.CreateReservationAsync(client, 1, "Other date", ScheduleDate.AddDays(1), new TimeOnly(8, 0), new TimeOnly(9, 0));

        var response = await client.GetAsync("/api/v1/reservations?room=1&date=2030-01-16");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var document = await response.ReadAsJsonAsync();
        var reservations = document.RootElement.EnumerateArray().ToArray();

        Assert.Equal(new[] { "09:00", "10:00", "11:00" }, reservations.Select(item => item.GetProperty("start").GetString()).ToArray());
        Assert.Equal([first, second, third], [.. reservations.Select(item => item.GetProperty("id").GetInt32())]);
    }

    [Theory]
    [InlineData("/api/v1/reservations?date=2030-01-16", "room")]
    [InlineData("/api/v1/reservations?room=1", "date")]
    public async Task MissingRoomOrDate_ReturnsValidationProblem(string path, string field)
    {
        await using var factory = new WebApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync(path);

        await IntegrationTestSupport.AssertProblemAsync(
            response,
            HttpStatusCode.BadRequest,
            "VALIDATION_ERROR",
            field);
    }

    [Fact]
    public async Task InvalidDate_ReturnsValidationProblem()
    {
        using var factory = new WebApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/v1/reservations?room=1&date=2030-02-30");

        await IntegrationTestSupport.AssertProblemAsync(
            response,
            HttpStatusCode.BadRequest,
            "VALIDATION_ERROR",
            "date");
    }

    [Fact]
    public async Task ExistingRoomWithoutReservations_ReturnsEmptyArray()
    {
        await using var factory = new WebApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/v1/reservations?room=1&date=2030-01-16");
        using var document = await response.ReadAsJsonAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(0, document.RootElement.GetArrayLength());
    }

    [Fact]
    public async Task UnknownRoom_ReturnsRoomNotFoundProblem()
    {
        using var factory = new WebApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/v1/reservations?room=999&date=2030-01-16");

        await IntegrationTestSupport.AssertProblemAsync(
            response,
            HttpStatusCode.NotFound,
            "ROOM_NOT_FOUND");
    }

    [Fact]
    public async Task ExistingReservation_ReturnsExactRepresentation()
    {
        await using var factory = new WebApplicationFactory();
        using var client = factory.CreateClient();
        var id = await IntegrationTestSupport.CreateReservationAsync(
            client,
            1,
            "Candidate interview",
            FutureDate,
            new TimeOnly(9, 0),
            new TimeOnly(10, 0));

        var response = await client.GetAsync($"/api/v1/reservations/{id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var document = await response.ReadAsJsonAsync();
        var reservation = document.RootElement;

        Assert.Equal(id, reservation.GetProperty("id").GetInt32());
        Assert.Equal("Candidate interview", reservation.GetProperty("title").GetString());
        Assert.Equal("2026-10-06", reservation.GetProperty("date").GetString());
        Assert.Equal("09:00", reservation.GetProperty("start").GetString());
        Assert.Equal("10:00", reservation.GetProperty("end").GetString());
        Assert.Equal("2026-10-05T02:00:00.0000000Z", reservation.GetProperty("createdAt").GetString());
        Assert.False(reservation.TryGetProperty("room", out _));
        Assert.False(reservation.TryGetProperty("roomId", out _));
    }

    [Fact]
    public async Task MissingReservation_ReturnsReservationNotFoundProblem()
    {
        await using var factory = new WebApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/v1/reservations/999");

        await IntegrationTestSupport.AssertProblemAsync(
            response,
            HttpStatusCode.NotFound,
            "RESERVATION_NOT_FOUND");
    }

    [Fact]
    public async Task ValidFutureReservation_ReturnsCreatedAndRetrievableLocation()
    {
        await using var factory = new WebApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsync(
            "/api/v1/reservations",
            HttpClientUtils.JsonContent(CreateValidRequest()));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        using var createdDocument = await response.ReadAsJsonAsync();
        var id = createdDocument.RootElement.GetProperty("id").GetInt32();
        Assert.Equal($"/api/v1/reservations/{id}", response.Headers.Location?.AbsolutePath);
        Assert.Equal("2026-10-05T02:00:00.0000000Z", createdDocument.RootElement.GetProperty("createdAt").GetString());

        var getResponse = await client.GetAsync(response.Headers.Location);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        using var getDocument = await getResponse.ReadAsJsonAsync();
        Assert.True(JsonElement.DeepEquals(createdDocument.RootElement, getDocument.RootElement));
    }

    [Fact]
    public async Task UnknownRoom_ReturnsRoomNotFoundWithoutInsert()
    {
        await using var factory = new WebApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsync(
            "/api/v1/reservations",
            HttpClientUtils.JsonContent(CreateValidRequest(room: 999)));

        await IntegrationTestSupport.AssertProblemAsync(
            response,
            HttpStatusCode.NotFound,
            "ROOM_NOT_FOUND");
        var getResponse = await client.GetAsync("/api/v1/reservations/1");
        await IntegrationTestSupport.AssertProblemAsync(
            getResponse,
            HttpStatusCode.NotFound,
            "RESERVATION_NOT_FOUND");
    }

    [Fact]
    public async Task InvalidOrMissingFields_ReturnValidationProblemWithoutInsert()
    {
        await using var factory = new WebApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsync(
            "/api/v1/reservations",
            HttpClientUtils.JsonContent(
                                     /*lang=json,strict*/
                                     """
                {"room":0,"title":"Planning","date":"invalid","start":"25:00","end":"09:00"}
                """));

        await IntegrationTestSupport.AssertProblemAsync(
            response,
            HttpStatusCode.BadRequest,
            "VALIDATION_ERROR",
            "room");
        using var document = await response.ReadAsJsonAsync();
        var errors = document.RootElement.GetProperty("errors");
        Assert.True(errors.TryGetProperty("date", out _));
        Assert.True(errors.TryGetProperty("start", out _));
        Assert.Empty(await IntegrationTestSupport.GetScheduleAsync(client, 1, FutureDate));
    }

    [Fact]
    public async Task MissingTitle_ReturnsValidationProblemWithoutInsert()
    {
        await using var factory = new WebApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsync(
            "/api/v1/reservations",
            HttpClientUtils.JsonContent(
                                     /*lang=json,strict*/
                                     """
                {"room":1,"date":"2026-10-06","start":"10:00","end":"11:00"}
                """));

        await IntegrationTestSupport.AssertProblemAsync(
            response,
            HttpStatusCode.BadRequest,
            "VALIDATION_ERROR",
            "title");
        Assert.Empty(await IntegrationTestSupport.GetScheduleAsync(client, 1, FutureDate));
    }

    [Theory]
    [InlineData("{\"room\":1")]
    [InlineData(/*lang=json,strict*/ "{\"room\":1,\"title\":\"Planning\",\"date\":\"2026-10-06\",\"start\":\"10:00\",\"end\":\"11:00\",\"unknown\":true}")]
    [InlineData(/*lang=json,strict*/ "{\"Room\":1,\"title\":\"Planning\",\"date\":\"2026-10-06\",\"start\":\"10:00\",\"end\":\"11:00\"}")]
    [InlineData(/*lang=json,strict*/ "{\"room\":\"1\",\"title\":\"Planning\",\"date\":\"2026-10-06\",\"start\":\"10:00\",\"end\":\"11:00\"}")]
    [InlineData(/*lang=json,strict*/ "{\"room\":1,\"room\":2,\"title\":\"Planning\",\"date\":\"2026-10-06\",\"start\":\"10:00\",\"end\":\"11:00\"}")]
    public async Task MalformedOrNonContractJson_ReturnsValidationProblem(string payload)
    {
        await using var factory = new WebApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsync(
            "/api/v1/reservations",
            HttpClientUtils.JsonContent(payload));

        await IntegrationTestSupport.AssertProblemAsync(
            response,
            HttpStatusCode.BadRequest,
            "VALIDATION_ERROR");
        Assert.Empty(await IntegrationTestSupport.GetScheduleAsync(client, 1, FutureDate));
    }

    [Fact]
    public async Task NonJsonContentType_ReturnsUnsupportedMediaType()
    {
        await using var factory = new WebApplicationFactory();
        using var client = factory.CreateClient();
        using var content = new StringContent(CreateValidRequest(), Encoding.UTF8, "text/plain");

        var response = await client.PostAsync("/api/v1/reservations", content);

        await IntegrationTestSupport.AssertProblemAsync(
            response,
            HttpStatusCode.UnsupportedMediaType,
            "UNSUPPORTED_MEDIA_TYPE");
        Assert.Empty(await IntegrationTestSupport.GetScheduleAsync(client, 1, FutureDate));
    }

    [Theory]
    [InlineData("01:59")]
    [InlineData("02:00")]
    public async Task NonFutureStart_ReturnsValidationProblem(string start)
    {
        await using var factory = new WebApplicationFactory();
        using var client = factory.CreateClient();
        var payload = $$"""
            {"room":1,"title":"Planning","date":"2026-10-05","start":"{{start}}","end":"03:00"}
            """;

        var response = await client.PostAsync(
            "/api/v1/reservations",
            HttpClientUtils.JsonContent(payload));

        await IntegrationTestSupport.AssertProblemAsync(
            response,
            HttpStatusCode.BadRequest,
            "VALIDATION_ERROR",
            "start");
    }

    [Fact]
    public async Task Overlap_ReturnsConflictWithoutDisclosingExistingData()
    {
        await using var factory = new WebApplicationFactory();
        using var client = factory.CreateClient();
        _ = await IntegrationTestSupport.CreateReservationAsync(
            client,
            1,
            "Confidential candidate",
            FutureDate,
            new TimeOnly(10, 0),
            new TimeOnly(11, 0));

        var response = await client.PostAsync(
            "/api/v1/reservations",
            HttpClientUtils.JsonContent(CreateValidRequest(start: "10:30", end: "11:30")));
        var payload = await response.Content.ReadAsStringAsync();

        await IntegrationTestSupport.AssertProblemAsync(
            response,
            HttpStatusCode.Conflict,
            "ROOM_TIME_CONFLICT");
        Assert.DoesNotContain("Confidential candidate", payload);
        Assert.DoesNotContain("10:00", payload);
        _ = Assert.Single(await IntegrationTestSupport.GetScheduleAsync(client, 1, FutureDate));
    }

    [Theory]
    [InlineData("09:00", "10:00")]
    [InlineData("11:00", "12:00")]
    public async Task AdjacentSlot_ReturnsCreated(string start, string end)
    {
        await using var factory = new WebApplicationFactory();
        using var client = factory.CreateClient();
        _ = await IntegrationTestSupport.CreateReservationAsync(
            client,
            1,
            "Existing",
            FutureDate,
            new TimeOnly(10, 0),
            new TimeOnly(11, 0));

        var response = await client.PostAsync(
            "/api/v1/reservations",
            HttpClientUtils.JsonContent(CreateValidRequest(start: start, end: end)));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal(2, (await IntegrationTestSupport.GetScheduleAsync(client, 1, FutureDate)).Length);
    }

    private static string CreateValidRequest(
        int room = 1,
        string title = "Planning",
        string date = "2026-10-06",
        string start = "10:00",
        string end = "11:00") =>
        JsonSerializer.Serialize(new
        {
            room,
            title,
            date,
            start,
            end
        });
}
