using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace RoomBooking.Api.Infrastructure;

internal static class ApiProblems
{
    private static readonly JsonSerializerOptions SerializerOptions =
        new(JsonSerializerDefaults.Web);

    public static ObjectResult Create(
        HttpContext httpContext,
        int status,
        string code,
        string title,
        string detail)
    {
        var problem = BaseProblem(httpContext, status, code, title, detail);
        return Result(problem, status);
    }

    public static ObjectResult Validation(
        HttpContext httpContext,
        IDictionary<string, string[]> errors)
    {
        var problem = new ValidationProblemDetails(errors)
        {
            Type = "urn:room-booking:error:validation-error",
            Title = "The request is invalid.",
            Status = StatusCodes.Status400BadRequest,
            Detail = "Correct the invalid fields and retry.",
            Instance = httpContext.Request.Path
        };
        AddExtensions(problem, httpContext, "VALIDATION_ERROR");
        return Result(problem, StatusCodes.Status400BadRequest);
    }

    public static ObjectResult Validation(
        HttpContext httpContext,
        ModelStateDictionary modelState)
    {
        var errors = modelState
            .Where(entry => entry.Value?.Errors.Count > 0)
            .ToDictionary(
                entry => NormalizeKey(entry.Key),
                _ => new[] { "The supplied value is invalid." });

        return Validation(httpContext, errors);
    }

    public static ProblemDetails BaseProblem(
        HttpContext httpContext,
        int status,
        string code,
        string title,
        string detail)
    {
        var slug = code.ToLowerInvariant().Replace('_', '-');
        var problem = new ProblemDetails
        {
            Type = $"urn:room-booking:error:{slug}",
            Title = title,
            Status = status,
            Detail = detail,
            Instance = httpContext.Request.Path
        };
        AddExtensions(problem, httpContext, code);
        return problem;
    }

    public static Task WriteAsync(
        HttpContext httpContext,
        ProblemDetails problem,
        CancellationToken cancellationToken = default)
    {
        httpContext.Response.ContentType = "application/problem+json";
        var payload = JsonSerializer.Serialize(
            problem,
            problem.GetType(),
            SerializerOptions);
        return httpContext.Response.WriteAsync(payload, cancellationToken);
    }

    private static ObjectResult Result(ProblemDetails problem, int status)
    {
        var result = new ObjectResult(problem)
        {
            StatusCode = status
        };
        result.ContentTypes.Add("application/problem+json");
        return result;
    }

    private static void AddExtensions(
        ProblemDetails problem,
        HttpContext httpContext,
        string code)
    {
        problem.Extensions["code"] = code;
        problem.Extensions["traceId"] = httpContext.TraceIdentifier;
    }

    private static string NormalizeKey(string key)
    {
        var separator = key.LastIndexOf('.');
        var value = separator >= 0 ? key[(separator + 1)..] : key;
        return string.IsNullOrEmpty(value)
            ? "request"
            : char.ToLowerInvariant(value[0]) + value[1..];
    }
}
