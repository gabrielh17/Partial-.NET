namespace RoomHub.Api.Domain.Bookings;

// Write-side contract. No EF Core types here; the implementation lives in Infrastructure.
public interface IBookingRepository
{
    Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    // The room's Active bookings, used by Booking.Create for the overlap rule (R7).
    Task<IReadOnlyList<Booking>> ListActiveForRoomAsync(Guid roomId, CancellationToken cancellationToken);

    // R9: the participant's Active bookings (any room) that start on or after fromUtc.
    // Booking.Create picks the ones on the same UTC day.
    Task<IReadOnlyList<Booking>> ListActiveForBookedByAsync(
        string bookedBy, DateTime fromUtc, CancellationToken cancellationToken);

    // Only marks the booking as new. Saving is the job of IUnitOfWork.
    void Add(Booking booking);
}
