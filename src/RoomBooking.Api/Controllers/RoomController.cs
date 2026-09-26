using Microsoft.AspNetCore.Mvc;
using RoomBooking.Api.ApiModels;
using RoomBooking.BusinessContracts;

namespace RoomBooking.Api.Controllers;

[ApiController]
[Route("api/v1/rooms")]
public sealed class RoomController(IRoomService roomService) : ControllerBase
{
    private readonly IRoomService _roomService = roomService;

    [HttpGet(Name = "ListRooms")]
    [ProducesResponseType<RoomData[]>(StatusCodes.Status200OK)]
    public async Task<ActionResult<RoomData[]>> ListRoomsAsync(
        CancellationToken cancellationToken)
    {
        var rooms = await _roomService.ListRoomsAsync(cancellationToken);
        return Ok(rooms.Select(room => new RoomData(
            room.Id,
            room.Name,
            room.Location,
            room.Description,
            room.Capacity)).ToArray());
    }
}
