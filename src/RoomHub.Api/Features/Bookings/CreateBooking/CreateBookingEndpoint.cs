using Microsoft.AspNetCore.Http.HttpResults;
using RoomHub.Api.Common;
using RoomHub.Api.Domain.Bookings;
using RoomHub.Api.Infrastructure;

namespace RoomHub.Api.Features.Bookings.CreateBooking;

public static class CreateBookingEndpoint
{
    private static readonly TimeSpan MaxDuration = TimeSpan.FromHours(4);

    public sealed record Request(string? BookedBy, DateTime StartUtc, DateTime EndUtc, int Attendees);

    public sealed record Response(Guid Id, Guid RoomId, string Status);

    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("/{roomId:guid}/bookings", Handle)
            .WithName("CreateBooking");

    private static Created<Response> Handle(
        Guid roomId,
        Request request,
        RoomRepository rooms,
        BookingRepository bookings,
        TimeProvider time)
    {
        // The room must exist: 404 comes before any validation.
        var room = rooms.Get(roomId)
                   ?? throw new BusinessException("RoomNotFound", "The room does not exist.", 404);

        // R2: bookedBy is required.
        if (string.IsNullOrWhiteSpace(request.BookedBy))
            throw new BusinessException("InvalidBookedBy", "BookedBy is required.", 400);

        var startUtc = ToUtc(request.StartUtc);
        var endUtc = ToUtc(request.EndUtc);

        // R3: the end is strictly after the start.
        if (endUtc <= startUtc)
            throw new BusinessException("InvalidRange", "The end must be after the start.", 400);

        // R4: attendees is strictly positive and at most the room capacity.
        if (request.Attendees <= 0)
            throw new BusinessException("InvalidAttendees", "Attendees must be positive.", 400);

        if (request.Attendees > room.Capacity)
            throw new BusinessException("AttendeesExceedCapacity", "The room does not have enough seats.", 400);

        // R5: no booking in the past. "Now" comes from TimeProvider.
        if (startUtc < time.GetUtcNow().UtcDateTime)
            throw new BusinessException("StartInPast", "A booking cannot start in the past.", 400);

        // R6: at most 4 hours; exactly 4 hours is allowed.
        if (endUtc - startUtc > MaxDuration)
            throw new BusinessException("DurationTooLong", "A booking can last at most 4 hours.", 400);

        // R7: only active bookings block the room, and the intervals are half-open,
        // so bookings that only touch (one ends when the other starts) do not overlap.
        var overlaps = bookings.ListForRoom(roomId).Any(existing =>
            existing.Status == BookingStatus.Active &&
            existing.StartUtc < endUtc &&
            startUtc < existing.EndUtc);

        if (overlaps)
            throw new BusinessException("RoomOccupied", "The room is occupied in this interval.", 409);

        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            RoomId = roomId,
            BookedBy = request.BookedBy.Trim(),
            StartUtc = startUtc,
            EndUtc = endUtc,
            Attendees = request.Attendees,
            Status = BookingStatus.Active
        };

        bookings.Add(booking);

        return TypedResults.Created(
            $"/api/bookings/{booking.Id}",
            new Response(booking.Id, booking.RoomId, booking.Status.ToString()));
    }

    // Dates in the contract are UTC. A value with an offset is converted,
    // and a value without one is treated as UTC.
    private static DateTime ToUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
    };
}
