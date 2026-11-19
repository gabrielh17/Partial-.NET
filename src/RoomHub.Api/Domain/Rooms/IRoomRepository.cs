namespace RoomHub.Api.Domain.Rooms;

// Write-side contract. No EF Core types here; the implementation lives in Infrastructure.
public interface IRoomRepository
{
    Task<Room?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    // Only marks the room as new. Saving is the job of IUnitOfWork.
    void Add(Room room);
}
