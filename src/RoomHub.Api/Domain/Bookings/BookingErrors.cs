using RoomHub.Api.Domain.Common;

namespace RoomHub.Api.Domain.Bookings;

public static class BookingErrors
{
    public static readonly Error NotFound =
        Error.NotFound("BookingNotFound", "The booking does not exist.");

    public static readonly Error BookedByRequired =
        Error.Validation("InvalidBookedBy", "BookedBy is required.");

    public static readonly Error InvalidRange =
        Error.Validation("InvalidRange", "The end must be after the start.");

    public static readonly Error InvalidAttendees =
        Error.Validation("InvalidAttendees", "Attendees must be positive.");

    public static readonly Error AttendeesExceedCapacity =
        Error.Validation("AttendeesExceedCapacity", "The room does not have enough seats.");

    public static readonly Error StartInPast =
        Error.Validation("StartInPast", "A booking cannot start in the past.");

    // R6 / R10: the limit depends on the room (4h, or 2h for capacity > 10).
    public static Error DurationTooLong(TimeSpan maxDuration) =>
        Error.Validation("DurationTooLong", $"A booking in this room can last at most {maxDuration.TotalHours:0.##} hours.");

    public static readonly Error RoomOccupied =
        Error.Conflict("RoomOccupied", "The room is occupied in this interval.");

    public static readonly Error DailyLimitReached =
        Error.Conflict("DailyLimitReached",
            $"A participant can have at most {Booking.MaxActiveBookingsPerDay} active bookings per day.");

    public static readonly Error AlreadyCancelled =
        Error.Conflict("BookingAlreadyCancelled", "The booking is already cancelled.");

    public static readonly Error TooLateToCancel =
        Error.Conflict("TooLateToCancel",
            $"A booking can be cancelled only if it starts at least {Booking.MinCancellationNotice.TotalMinutes:0} minutes from now.");
}
