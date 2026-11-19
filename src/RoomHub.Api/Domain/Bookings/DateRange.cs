using RoomHub.Api.Domain.Common;

namespace RoomHub.Api.Domain.Bookings;

// Value object: defined by its two dates, immutable from the outside.
public sealed record DateRange
{
    // For EF Core. Outside code uses DateRange.Create.
    private DateRange()
    {
    }

    public DateTime StartUtc { get; private set; }
    public DateTime EndUtc { get; private set; }

    public TimeSpan Duration => EndUtc - StartUtc;

    public static Result<DateRange> Create(DateTime start, DateTime end)
    {
        var startUtc = ToUtc(start);
        var endUtc = ToUtc(end);

        // R3: the end is strictly after the start.
        if (endUtc <= startUtc)
            return BookingErrors.InvalidRange;

        return new DateRange { StartUtc = startUtc, EndUtc = endUtc };
    }

    // R7: half-open intervals. 10-12 and 12-14 only touch, so they do not overlap.
    public bool Overlaps(DateRange other) =>
        StartUtc < other.EndUtc && other.StartUtc < EndUtc;

    // Dates in the contract are UTC. A value with an offset is converted,
    // and a value without one is treated as UTC.
    private static DateTime ToUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
    };
}
