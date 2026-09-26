using System.Net;
using System.Text.Json;
using RoomBooking.Common;

namespace RoomBooking.IntegrationTests;

internal static class IntegrationTestSupport
{
    public static async Task<int> CreateReservationAsync(
        HttpClient client,
        int roomId,
        string title,
        DateOnly date,
        TimeOnly start,
        TimeOnly end)
    {
        var payload = JsonSerializer.Serialize(new
        {
            room = roomId,
            title,
            date = date.ToString("yyyy-MM-dd"),
            start = start.ToString("HH:mm"),
            end = end.ToString("HH:mm")
        });
        var response = await client.PostAsync("/api/v1/reservations", HttpClientUtils.JsonContent(payload));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        using var document = await response.ReadAsJsonAsync();
        return document.RootElement.GetProperty("id").GetInt32();
    }

    public static async Task<JsonElement[]> GetScheduleAsync(
        HttpClient client,
        int roomId,
        DateOnly date)
    {
        var response = await client.GetAsync(
            $"/api/v1/reservations?room={roomId}&date={date:yyyy-MM-dd}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var document = await response.ReadAsJsonAsync();
        return [.. document.RootElement.EnumerateArray().Select(item => item.Clone())];
    }

    public static async Task AssertProblemAsync(
        HttpResponseMessage response,
        HttpStatusCode expectedStatus,
        string expectedCode,
        string? expectedField = null)
    {
        Assert.Equal(expectedStatus, response.StatusCode);
        Assert.Equal(
            "application/problem+json",
            response.Content.Headers.ContentType?.MediaType);

        using var document = await response.ReadAsJsonAsync();
        var root = document.RootElement;
        Assert.Equal((int)expectedStatus, root.GetProperty("status").GetInt32());
        Assert.Equal(expectedCode, root.GetProperty("code").GetString());
        Assert.False(string.IsNullOrWhiteSpace(root.GetProperty("type").GetString()));
        Assert.False(string.IsNullOrWhiteSpace(root.GetProperty("title").GetString()));
        Assert.False(string.IsNullOrWhiteSpace(root.GetProperty("instance").GetString()));
        Assert.False(string.IsNullOrWhiteSpace(root.GetProperty("traceId").GetString()));

        if (expectedField is not null)
        {
            Assert.True(root.GetProperty("errors").TryGetProperty(expectedField, out _));
        }
    }
}
