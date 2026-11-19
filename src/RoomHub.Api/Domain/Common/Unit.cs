namespace RoomHub.Api.Domain.Common;

// "No data" for operations that only succeed or fail: Result<Unit>.
public readonly record struct Unit
{
    public static readonly Unit Value = new();
}
