using System.Text;
using System.Text.Json;

namespace RoomBooking.Common;

public static class HttpClientUtils
{
    public static StringContent JsonContent(string json) =>
        new(json, Encoding.UTF8, "application/json");

    public static async Task<JsonDocument> ReadAsJsonAsync(this HttpResponseMessage response) =>
        JsonDocument.Parse(await response.Content.ReadAsStringAsync());
}
