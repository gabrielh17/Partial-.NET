using RoomHub.Api.Domain.Common;

namespace RoomHub.Api.Common.Cqrs;

// One handler per command: one use case. Written by hand (no MediatR)
// so the mechanism is visible.
public interface ICommandHandler<in TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    Task<Result<TResult>> Handle(TCommand command, CancellationToken cancellationToken);
}
