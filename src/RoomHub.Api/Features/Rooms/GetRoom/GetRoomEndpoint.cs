using Microsoft.AspNetCore.Http.HttpResults;
using RoomHub.Api.Common;
using RoomHub.Api.Infrastructure;

namespace RoomHub.Api.Features.Rooms.GetRoom;

public static class GetRoomEndpoint
{
    // Each slice has its own Response, even if it looks like the one in CreateRoom,
    // so the two can evolve independently.
    public sealed record Response(Guid Id, string Name, int Capacity);

    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("/{id:guid}", Handle)
            .WithName("GetRoom");

    private static Ok<Response> Handle(Guid id, RoomRepository rooms)
    {
        var room = rooms.Get(id)
                   ?? throw new BusinessException("RoomNotFound", "The room does not exist.", 404);

        return TypedResults.Ok(new Response(room.Id, room.Name, room.Capacity));
    }
}
