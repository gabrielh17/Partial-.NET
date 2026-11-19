using Microsoft.EntityFrameworkCore;
using RoomHub.Api.Domain.Bookings;

namespace RoomHub.Api.Infrastructure.Persistence.Repositories;

public sealed class BookingRepository(RoomHubDbContext db) : IBookingRepository
{
    public Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        db.Bookings.FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

    // Cancelled bookings free their interval (R8), so only Active ones are loaded.
    public async Task<IReadOnlyList<Booking>> ListActiveForRoomAsync(
        Guid roomId, CancellationToken cancellationToken) =>
        await db.Bookings
            .Where(b => b.RoomId == roomId && b.Status == BookingStatus.Active)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Booking>> ListActiveForBookedByAsync(
        string bookedBy, DateTime fromUtc, CancellationToken cancellationToken) =>
        await db.Bookings
            .Where(b => b.BookedBy == bookedBy &&
                        b.Status == BookingStatus.Active &&
                        b.Range.StartUtc >= fromUtc)
            .ToListAsync(cancellationToken);

    // Only marks. No SQL runs until IUnitOfWork.SaveChangesAsync.
    public void Add(Booking booking) => db.Bookings.Add(booking);
}
