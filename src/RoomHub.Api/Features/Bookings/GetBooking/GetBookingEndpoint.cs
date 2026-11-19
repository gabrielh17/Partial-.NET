using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using RoomHub.Api.Common;
using RoomHub.Api.Domain.Bookings;
using RoomHub.Api.Infrastructure;

namespace RoomHub.Api.Features.Bookings.GetBooking;

public static class GetBookingEndpoint
{
    public sealed record Response(
        Guid Id,
        Guid RoomId,
        string BookedBy,
        DateTime StartUtc,
        DateTime EndUtc,
        int Attendees,
        BookingStatus Status);

    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("/{id:guid}", Handle)
            .WithName("GetBooking");

    // Read side: DbContext directly with a projection; the repository is for commands.
    private static async Task<Results<Ok<Response>, ProblemHttpResult>> Handle(
        Guid id,
        RoomHubDbContext db,
        CancellationToken ct)
    {
        var booking = await db.Bookings
            .AsNoTracking()
            .Where(b => b.Id == id)
            .Select(b => new Response(
                b.Id, b.RoomId, b.BookedBy, b.Range.StartUtc, b.Range.EndUtc, b.Attendees, b.Status))
            .FirstOrDefaultAsync(ct);

        if (booking is null)
            return BookingErrors.NotFound.ToProblem();

        return TypedResults.Ok(booking);
    }
}
