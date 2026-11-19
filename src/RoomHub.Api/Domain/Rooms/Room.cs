using RoomHub.Api.Domain.Common;

namespace RoomHub.Api.Domain.Rooms;

public sealed class Room
{
    // For EF Core. Outside code uses Room.Create.
    private Room()
    {
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public int Capacity { get; private set; }

    // The only way to get a Room: it cannot exist in an invalid state.
    public static Result<Room> Create(string? name, int capacity)
    {
        // R1: non-empty name.
        if (string.IsNullOrWhiteSpace(name))
            return RoomErrors.NameRequired;

        // R1: strictly positive capacity.
        if (capacity <= 0)
            return RoomErrors.InvalidCapacity;

        return new Room
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Capacity = capacity
        };
    }

    // R4 (upper bound): the room decides whether it has enough seats.
    public bool CanFit(int attendees) => attendees <= Capacity;
}
