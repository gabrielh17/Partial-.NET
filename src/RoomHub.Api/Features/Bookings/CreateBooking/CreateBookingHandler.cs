using RoomHub.Api.Common.Cqrs;
using RoomHub.Api.Domain.Bookings;
using RoomHub.Api.Domain.Common;
using RoomHub.Api.Domain.Rooms;

namespace RoomHub.Api.Features.Bookings.CreateBooking;

public sealed record CreateBookingCommand(
    Guid RoomId,
    string? BookedBy,
    DateTime StartUtc,
    DateTime EndUtc,
    int Attendees) : ICommand<Booking>;

public sealed class CreateBookingHandler(
    IRoomRepository rooms,
    IBookingRepository bookings,
    IUnitOfWork unitOfWork,
    TimeProvider time)
    : ICommandHandler<CreateBookingCommand, Booking>
{
    public async Task<Result<Booking>> Handle(CreateBookingCommand command, CancellationToken cancellationToken)
    {
        // The room must exist: 404 comes before any validation.
        var room = await rooms.GetByIdAsync(command.RoomId, cancellationToken);
        if (room is null)
            return RoomErrors.NotFound;

        var activeBookings = await bookings.ListActiveForRoomAsync(room.Id, cancellationToken);

        var nowUtc = time.GetUtcNow().UtcDateTime;

        // R9: the participant's active bookings, from today's UTC date on. A new booking
        // cannot start in the past (R5), so earlier days can never be "the same day".
        IReadOnlyList<Booking> participantBookings = string.IsNullOrWhiteSpace(command.BookedBy)
            ? []
            : await bookings.ListActiveForBookedByAsync(command.BookedBy.Trim(), nowUtc.Date, cancellationToken);

        // R2-R7 and R9-R10 are checked in Booking.Create. "Now" comes from TimeProvider (R5).
        var result = Booking.Create(
            room,
            command.BookedBy,
            command.StartUtc,
            command.EndUtc,
            command.Attendees,
            nowUtc,
            activeBookings,
            participantBookings);

        if (result.IsFailure)
            return result.Error;

        bookings.Add(result.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return result.Value;
    }
}
