namespace RoomHub.Api.Domain.Common;

// Saves everything changed during a request. RoomHubDbContext implements it;
// the interface keeps EF Core out of the handlers.
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
