using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using RoomBooking.Api.ApiModels;
using RoomBooking.Api.Infrastructure;
using RoomBooking.BusinessContracts;
using RoomBooking.Common;
using CreateReservationRequest = RoomBooking.Api.ApiModels.CreateReservationRequest;

namespace RoomBooking.Api.Controllers;

[ApiController]
[Route("api/v1")]
public sealed class ReservationController : ControllerBase
{
    private readonly IReservationService _reservationService;
    private readonly TimeProvider _timeProvider;
    private const string DateFormat = "yyyy-MM-dd";
    private const string TimeFormat = "HH:mm";

    public ReservationController(
        IReservationService reservationService,
        TimeProvider timeProvider)
    {
        _reservationService = reservationService;
        _timeProvider = timeProvider;
    }

    [HttpGet("reservations", Name = "ListRoomReservations")]
    [ProducesResponseType<ReservationData[]>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReservationData[]>> ListForRoomAsync(
        [FromQuery] int room,
        [FromQuery] string? date,
        CancellationToken cancellationToken)
    {
        var errors = new Dictionary<string, string[]>();
        if (room <= 0)
        {
            errors["room"] = ["room must be a positive integer."];
        }

        if (!DateOnly.TryParseExact(
                date,
                DateFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var parsedDate))
        {
            errors["date"] = [$"date must use {DateFormat} and be a valid date."];
        }

        if (errors.Count > 0)
        {
            return ApiProblems.Validation(HttpContext, errors);
        }

        var result = await _reservationService.ListForRoomAsync(
            room,
            parsedDate,
            cancellationToken);

        if (!result.RoomExists)
        {
            return ApiProblems.Create(
                HttpContext,
                StatusCodes.Status404NotFound,
                "ROOM_NOT_FOUND",
                "The room was not found.",
                "Choose an existing room and retry.");
        }

        return Ok(result.Reservations.Select(MapToData).ToArray());
    }

    [HttpGet("reservations/{id:int}", Name = "GetReservationById")]
    [ProducesResponseType<ReservationData>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReservationData>> GetByIdAsync(
        int id,
        CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            return ApiProblems.Validation(
                HttpContext,
                new Dictionary<string, string[]>
                {
                    ["id"] = ["id must be a positive integer."]
                });
        }

        var reservation = await _reservationService.GetByIdAsync(id, cancellationToken);
        return reservation is null
            ? ApiProblems.Create(
                HttpContext,
                StatusCodes.Status404NotFound,
                "RESERVATION_NOT_FOUND",
                "The reservation was not found.",
                "Choose an existing reservation and retry.")
            : Ok(MapToData(reservation));
    }

    [HttpPost("reservations", Name = "CreateReservation")]
    [Consumes("application/json")]
    [ProducesResponseType<ReservationData>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status415UnsupportedMediaType)]
    public async Task<ActionResult<ReservationData>> CreateAsync(
        [FromBody] CreateReservationRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryParseRequest(
                request,
                _timeProvider,
                out var reservation,
                out var errors))
        {
            return ApiProblems.Validation(HttpContext, errors);
        }

        var result = await _reservationService.CreateAsync(
            reservation!,
            cancellationToken);

        return result.Outcome switch
        {
            CreateReservationOutcome.Created => CreatedAtRoute(
                "GetReservationById",
                new { id = result.Reservation!.Id },
                MapToData(result.Reservation)),
            CreateReservationOutcome.RoomNotFound => ApiProblems.Create(
                HttpContext,
                StatusCodes.Status404NotFound,
                "ROOM_NOT_FOUND",
                "The room was not found.",
                "Choose an existing room and retry."),
            CreateReservationOutcome.TimeConflict => ApiProblems.Create(
                HttpContext,
                StatusCodes.Status409Conflict,
                "ROOM_TIME_CONFLICT",
                "The room is already reserved for part of that time.",
                "Choose another time or room and retry."),
            _ => throw new InvalidOperationException("Unknown reservation creation outcome.")
        };
    }

    private static ReservationData MapToData(ReservationModel reservation) =>
        new(
            reservation.Id,
            reservation.Title,
            reservation.Date.ToString(DateFormat),
            reservation.Start.ToString(TimeFormat),
            reservation.End.ToString(TimeFormat),
            reservation.CreatedAt.ToUniversalTime().ToString("O"));

    private static bool TryParseRequest(
        CreateReservationRequest request,
        TimeProvider timeProvider,
        out CreateReservationModel? reservation,
        out Dictionary<string, string[]> errors)
    {
        errors = [];

        if (request.Room <= 0)
        {
            errors["room"] = ["room must be a positive integer."];
        }

        if (!DateOnly.TryParseExact(
                request.Date,
                DateFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var date))
        {
            errors["date"] = [$"date must use {DateFormat} and be a valid date."];
        }

        if (!TimeOnly.TryParseExact(
                request.Start,
                TimeFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var start))
        {
            errors["start"] = [$"start must use {TimeFormat}."];
        }

        if (!TimeOnly.TryParseExact(
                request.End,
                TimeFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var end))
        {
            errors["end"] = [$"end must use {TimeFormat}."];
        }

        if (!errors.ContainsKey("start") &&
            !errors.ContainsKey("end") &&
            start >= end)
        {
            errors["end"] = ["end must be later than start on the same date."];
        }

        if (!errors.ContainsKey("date") &&
            !errors.ContainsKey("start") &&
            !DateTimeUtils.IsFuture(date, start, timeProvider))
        {
            errors["start"] = ["start must be in the future."];
        }

        if (errors.Count > 0)
        {
            reservation = null;
            return false;
        }

        reservation = new CreateReservationModel(request.Room, request.Title, date, start, end);
        return true;
    }
}