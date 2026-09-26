using Microsoft.AspNetCore.Diagnostics;

namespace RoomBooking.Api.Infrastructure;

internal sealed class ApiExceptionHandler(ILogger<ApiExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is OperationCanceledException &&
            httpContext.RequestAborted.IsCancellationRequested)
        {
            return false;
        }

        logger.LogError(exception, "Unhandled API exception.");
        var problem = ApiProblems.BaseProblem(
            httpContext,
            StatusCodes.Status500InternalServerError,
            "INTERNAL_ERROR",
            "An unexpected error occurred.",
            "Retry the request later.");

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await ApiProblems.WriteAsync(
            httpContext,
            problem,
            cancellationToken);
        return true;
    }
}
