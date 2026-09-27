using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using RoomBooking.Api.Infrastructure;
using RoomBooking.BusinessContracts;
using RoomBooking.BusinessServices;
using RoomBooking.DataAccess;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("RoomBooking")
    ?? throw new InvalidOperationException("Connection string 'RoomBooking' is not configured.");

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = false;
        options.JsonSerializerOptions.NumberHandling = JsonNumberHandling.Strict;
        options.JsonSerializerOptions.UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow;
        options.JsonSerializerOptions.AllowDuplicateProperties = false;
    });
builder.Services.Configure<Microsoft.AspNetCore.Mvc.ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
        ApiProblems.Validation(context.HttpContext, context.ModelState);
});
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddRoomBookingDataAccess(connectionString);
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<IRoomService, RoomService>();
builder.Services.AddScoped<IReservationService, ReservationService>();

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<RoomBookingDbContext>();
    _ = await dbContext.Database.EnsureCreatedAsync();
    _ = await dbContext.Database.ExecuteSqlRawAsync("PRAGMA journal_mode=WAL;");
}

app.UseExceptionHandler();
app.UseStatusCodePages(async statusCodeContext =>
{
    var response = statusCodeContext.HttpContext.Response;
    if (response.HasStarted || response.ContentLength is > 0)
    {
        return;
    }

    var (code, title, detail) = response.StatusCode switch
    {
        StatusCodes.Status404NotFound => (
            "NOT_FOUND",
            "The requested resource was not found.",
            "Check the request path and retry."),
        StatusCodes.Status415UnsupportedMediaType => (
            "UNSUPPORTED_MEDIA_TYPE",
            "The media type is not supported.",
            "Send the request as application/json."),
        _ => (
            "HTTP_ERROR",
            "The request could not be completed.",
            "Correct the request and retry.")
    };

    var problem = ApiProblems.BaseProblem(
        statusCodeContext.HttpContext,
        response.StatusCode,
        code,
        title,
        detail);
    await ApiProblems.WriteAsync(statusCodeContext.HttpContext, problem);
});

if (app.Environment.IsDevelopment())
{
    _ = app.MapOpenApi();
    _ = app.MapScalarApiReference();
}

app.MapControllers();

app.Run();

/// <summary>
/// Provides the application entry point type for integration hosting.
/// </summary>
public partial class Program;
