using RoomHub.Api.Common.Cqrs;
using RoomHub.Api.Domain.Bookings;
using RoomHub.Api.Domain.Common;

namespace RoomHub.Api.Features.Bookings.CancelBooking;

public sealed record CancelBookingCommand(Guid BookingId) : ICommand<Unit>;

public sealed class CancelBookingHandler(
    IBookingRepository bookings,
    IUnitOfWork unitOfWork,
    TimeProvider time)
    : ICommandHandler<CancelBookingCommand, Unit>
{
    public async Task<Result<Unit>> Handle(CancelBookingCommand command, CancellationToken cancellationToken)
    {
        // R8: a missing booking gives 404.
        var booking = await bookings.GetByIdAsync(command.BookingId, cancellationToken);
        if (booking is null)
            return BookingErrors.NotFound;

        // R8 (409 for a second cancel) and R11 (409 if it starts in less than 60 minutes)
        // live in Booking.Cancel. "Now" comes from TimeProvider.
        var result = booking.Cancel(time.GetUtcNow().UtcDateTime);
        if (result.IsFailure)
            return result.Error;

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
