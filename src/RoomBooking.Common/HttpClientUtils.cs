using System.Text;
using System.Text.Json;

namespace RoomBooking.Common;

/// <summary>
/// Provides JSON helpers for HTTP client requests and responses.
/// </summary>
public static class HttpClientUtils
{
    /// <summary>
    /// Creates UTF-8 JSON HTTP content from a serialized payload.
    /// </summary>
    /// <param name="json">The serialized JSON payload.</param>
    /// <returns>The HTTP content with an <c>application/json</c> media type.</returns>
    public static StringContent JsonContent(string json) =>
        new(json, Encoding.UTF8, "application/json");

    /// <summary>
    /// Parses the response content as a JSON document.
    /// </summary>
    /// <param name="response">The HTTP response containing JSON content.</param>
    /// <returns>The parsed JSON document.</returns>
    public static async Task<JsonDocument> ReadAsJsonAsync(this HttpResponseMessage response) =>
        JsonDocument.Parse(await response.Content.ReadAsStringAsync());
}
