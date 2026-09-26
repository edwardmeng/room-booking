using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RoomBooking.Api.Infrastructure;

namespace RoomBooking.UnitTests;

public sealed class ProblemDetailsTests
{
    [Theory]
    [InlineData(400, "VALIDATION_ERROR")]
    [InlineData(404, "ROOM_NOT_FOUND")]
    [InlineData(404, "RESERVATION_NOT_FOUND")]
    [InlineData(409, "ROOM_TIME_CONFLICT")]
    [InlineData(415, "UNSUPPORTED_MEDIA_TYPE")]
    [InlineData(500, "INTERNAL_ERROR")]
    public void ApplicationFailure_MapsToStableProblemDetails(
        int expectedStatus,
        string expectedCode)
    {
        var httpContext = CreateHttpContext();
        var result = ApiProblems.Create(
            httpContext,
            expectedStatus,
            expectedCode,
            "Public title.",
            "Public detail.");
        var problem = Assert.IsType<ProblemDetails>(result.Value);

        Assert.Equal(expectedStatus, result.StatusCode);
        Assert.Equal(expectedStatus, problem.Status);
        Assert.Equal(expectedCode, problem.Extensions["code"]);
        Assert.Equal("trace-123", problem.Extensions["traceId"]);
        Assert.Equal("/api/v1/reservations", problem.Instance);
        Assert.False(string.IsNullOrWhiteSpace(problem.Type));
    }

    [Fact]
    public async Task UnexpectedFailure_DoesNotExposeExceptionDetails()
    {
        var logger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<ApiExceptionHandler>();
        var handler = new ApiExceptionHandler(logger);
        var httpContext = CreateHttpContext();
        httpContext.Response.Body = new MemoryStream();

        var handled = await handler.TryHandleAsync(
            httpContext,
            new InvalidOperationException("secret SQL and connection string"),
            CancellationToken.None);
        httpContext.Response.Body.Position = 0;
        var payload = await new StreamReader(httpContext.Response.Body).ReadToEndAsync();

        Assert.True(handled);
        Assert.Equal(500, httpContext.Response.StatusCode);
        Assert.Contains("INTERNAL_ERROR", payload);
        Assert.DoesNotContain("secret SQL", payload);
    }

    [Fact]
    public void ValidationFailure_ContainsFieldErrors()
    {
        var result = ApiProblems.Validation(
            CreateHttpContext(),
            new Dictionary<string, string[]>
            {
                ["date"] = ["date is invalid."]
            });
        var problem = Assert.IsType<ValidationProblemDetails>(result.Value);

        Assert.Equal("VALIDATION_ERROR", problem.Extensions["code"]);
        Assert.Equal(new[] { "date is invalid." }, problem.Errors["date"]);
    }

    private static DefaultHttpContext CreateHttpContext()
    {
        return new DefaultHttpContext
        {
            TraceIdentifier = "trace-123",
            Request =
            {
                Path = "/api/v1/reservations"
            }
        };
    }
}