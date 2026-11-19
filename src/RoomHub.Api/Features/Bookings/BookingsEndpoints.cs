using RoomHub.Api.Features.Bookings.CancelBooking;
using RoomHub.Api.Features.Bookings.CreateBooking;
using RoomHub.Api.Features.Bookings.GetBooking;

namespace RoomHub.Api.Features.Bookings;

public static class BookingsEndpoints
{
    public static IEndpointRouteBuilder MapBookings(this IEndpointRouteBuilder app)
    {
        // A booking is created under its room: POST /api/rooms/{roomId}/bookings.
        var roomBookings = app.MapGroup("/api/rooms").WithTags("Bookings");
        CreateBookingEndpoint.Map(roomBookings);

        var group = app.MapGroup("/api/bookings").WithTags("Bookings");
        GetBookingEndpoint.Map(group);
        CancelBookingEndpoint.Map(group);

        return app;
    }
}
