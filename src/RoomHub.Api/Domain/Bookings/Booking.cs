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
    // R9: at most 2 active bookings per participant per UTC day, across all rooms.
    public const int MaxActiveBookingsPerDay = 2;

    // R11: a booking can be cancelled only if it starts at least this far from now.
    public static readonly TimeSpan MinCancellationNotice = TimeSpan.FromMinutes(60);

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

    // The only way to get a Booking. "Now" comes from the caller (TimeProvider).
    // The existing bookings are passed in so the overlap (R7) and daily limit (R9)
    // rules live here:
    //  - existingRoomBookings: the room's bookings;
    //  - existingParticipantBookings: the participant's bookings (any room).
    public static Result<Booking> Create(
        Room room,
        string? bookedBy,
        DateTime start,
        DateTime end,
        int attendees,
        DateTime nowUtc,
        IEnumerable<Booking> existingRoomBookings,
        IEnumerable<Booking> existingParticipantBookings)
    {
        // R2: bookedBy is required.
        if (string.IsNullOrWhiteSpace(bookedBy))
            return BookingErrors.BookedByRequired;

        var participant = bookedBy.Trim();

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

        // R6 + R10: the room decides the maximum duration (4h, or 2h if capacity > 10).
        if (range.Duration > room.MaxBookingDuration)
            return BookingErrors.DurationTooLong(room.MaxBookingDuration);

        // R7: only active bookings of the same room block the interval.
        var occupied = existingRoomBookings.Any(existing =>
            existing.RoomId == room.Id &&
            existing.IsActive &&
            existing.Range.Overlaps(range));

        if (occupied)
            return BookingErrors.RoomOccupied;

        // R9: the "day" is the UTC date of startUtc. Cancelled bookings do not count,
        // and bookings on other days do not matter.
        var activeOnSameDay = existingParticipantBookings.Count(existing =>
            existing.BookedBy == participant &&
            existing.IsActive &&
            existing.Range.StartUtc.Date == range.StartUtc.Date);

        if (activeOnSameDay >= MaxActiveBookingsPerDay)
            return BookingErrors.DailyLimitReached;

        return new Booking
        {
            Id = Guid.NewGuid(),
            RoomId = room.Id,
            BookedBy = participant,
            Range = range,
            Attendees = attendees,
            Status = BookingStatus.Active
        };
    }

    // R8: Active -> Cancelled. A second cancel is a conflict.
    // A cancelled booking is no longer IsActive, so Create ignores it (the interval is freed).
    // R11: allowed only if the booking starts at least 60 minutes after nowUtc.
    public Result<Unit> Cancel(DateTime nowUtc)
    {
        if (Status == BookingStatus.Cancelled)
            return BookingErrors.AlreadyCancelled;

        if (Range.StartUtc - nowUtc < MinCancellationNotice)
            return BookingErrors.TooLateToCancel;

        Status = BookingStatus.Cancelled;
        return Unit.Value;
    }
}
