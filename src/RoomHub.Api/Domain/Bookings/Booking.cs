using RoomHub.Api.Domain.Common;
using RoomHub.Api.Domain.Rooms;

namespace RoomHub.Api.Domain.Bookings;

public enum BookingStatus
{
    Active,
    Cancelled
}

public sealed class Booking
{
    public static readonly TimeSpan MaxDuration = TimeSpan.FromHours(4);

    // For EF Core. Outside code uses Booking.Create.
    private Booking()
    {
    }

    public Guid Id { get; private set; }
    public Guid RoomId { get; private set; }
    public string BookedBy { get; private set; } = string.Empty;
    public DateRange Range { get; private set; } = null!;
    public int Attendees { get; private set; }
    public BookingStatus Status { get; private set; }

    public bool IsActive => Status == BookingStatus.Active;

    // The only way to get a Booking. "Now" comes from the caller (TimeProvider),
    // and the room's existing bookings are passed in so the overlap rule lives here.
    public static Result<Booking> Create(
        Room room,
        string? bookedBy,
        DateTime start,
        DateTime end,
        int attendees,
        DateTime nowUtc,
        IEnumerable<Booking> existingRoomBookings)
    {
        // R2: bookedBy is required.
        if (string.IsNullOrWhiteSpace(bookedBy))
            return BookingErrors.BookedByRequired;

        // R3: checked inside DateRange.Create.
        var rangeResult = DateRange.Create(start, end);
        if (rangeResult.IsFailure)
            return rangeResult.Error;

        var range = rangeResult.Value;

        // R4: strictly positive and at most the room capacity.
        if (attendees <= 0)
            return BookingErrors.InvalidAttendees;

        if (!room.CanFit(attendees))
            return BookingErrors.AttendeesExceedCapacity;

        // R5: a booking cannot start in the past.
        if (range.StartUtc < nowUtc)
            return BookingErrors.StartInPast;

        // R6: at most 4 hours; exactly 4 hours is allowed.
        if (range.Duration > MaxDuration)
            return BookingErrors.DurationTooLong;

        // R7: only active bookings of the same room block the interval.
        var occupied = existingRoomBookings.Any(existing =>
            existing.RoomId == room.Id &&
            existing.IsActive &&
            existing.Range.Overlaps(range));

        if (occupied)
            return BookingErrors.RoomOccupied;

        return new Booking
        {
            Id = Guid.NewGuid(),
            RoomId = room.Id,
            BookedBy = bookedBy.Trim(),
            Range = range,
            Attendees = attendees,
            Status = BookingStatus.Active
        };
    }

    // R8: Active -> Cancelled. A second cancel is a conflict.
    // A cancelled booking is no longer IsActive, so Create ignores it (the interval is freed).
    public Result<Unit> Cancel()
    {
        if (Status == BookingStatus.Cancelled)
            return BookingErrors.AlreadyCancelled;

        Status = BookingStatus.Cancelled;
        return Unit.Value;
    }
}
