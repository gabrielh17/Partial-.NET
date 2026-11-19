using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using RoomHub.Api.Common;
using RoomHub.Api.Domain.Rooms;
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

    // Read side: DbContext directly with a projection; the repository is for commands.
    private static async Task<Results<Ok<Response>, ProblemHttpResult>> Handle(
        Guid id,
        RoomHubDbContext db,
        CancellationToken ct)
    {
        var room = await db.Rooms
            .AsNoTracking()
            .Where(r => r.Id == id)
            .Select(r => new Response(r.Id, r.Name, r.Capacity))
            .FirstOrDefaultAsync(ct);

        if (room is null)
            return RoomErrors.NotFound.ToProblem();

        return TypedResults.Ok(room);
    }
}
