using Microsoft.AspNetCore.Http.HttpResults;
using RoomHub.Api.Common;
using RoomHub.Api.Domain.Bookings;
using RoomHub.Api.Infrastructure;

namespace RoomHub.Api.Features.Bookings.CancelBooking;

public static class CancelBookingEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("/{id:guid}/cancel", Handle)
            .WithName("CancelBooking");

    private static NoContent Handle(Guid id, BookingRepository bookings)
    {
        // R8: a missing booking gives 404.
        var booking = bookings.Get(id)
                      ?? throw new BusinessException("BookingNotFound", "The booking does not exist.", 404);

        // R8: cancelling an already cancelled booking gives 409.
        if (booking.Status == BookingStatus.Cancelled)
            throw new BusinessException("BookingAlreadyCancelled", "The booking is already cancelled.", 409);

        // R8: Active -> Cancelled. CreateBooking ignores cancelled bookings, so the interval is freed.
        booking.Status = BookingStatus.Cancelled;

        return TypedResults.NoContent();
    }
}
