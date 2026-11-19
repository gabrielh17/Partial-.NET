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

        // R2-R7 are checked in Booking.Create. "Now" comes from TimeProvider (R5).
        var result = Booking.Create(
            room,
            command.BookedBy,
            command.StartUtc,
            command.EndUtc,
            command.Attendees,
            time.GetUtcNow().UtcDateTime,
            activeBookings);

        if (result.IsFailure)
            return result.Error;

        bookings.Add(result.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return result.Value;
    }
}
