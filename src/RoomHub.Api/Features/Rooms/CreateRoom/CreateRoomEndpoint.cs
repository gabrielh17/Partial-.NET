using Microsoft.AspNetCore.Http.HttpResults;
using RoomHub.Api.Common;
using RoomHub.Api.Domain.Rooms;
using RoomHub.Api.Infrastructure;

namespace RoomHub.Api.Features.Rooms.CreateRoom;

public static class CreateRoomEndpoint
{
    public sealed record Request(string? Name, int Capacity);

    public sealed record Response(Guid Id, string Name, int Capacity);

    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("/", Handle)
            .WithName("CreateRoom");

    private static Created<Response> Handle(Request request, RoomRepository rooms)
    {
        // R1: non-empty name.
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new BusinessException("InvalidName", "Room name is required.", 400);

        // R1: strictly positive capacity.
        if (request.Capacity <= 0)
            throw new BusinessException("InvalidCapacity", "Capacity must be positive.", 400);

        var room = new Room
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Capacity = request.Capacity
        };

        rooms.Add(room);

        return TypedResults.Created(
            $"/api/rooms/{room.Id}",
            new Response(room.Id, room.Name, room.Capacity));
    }
}
