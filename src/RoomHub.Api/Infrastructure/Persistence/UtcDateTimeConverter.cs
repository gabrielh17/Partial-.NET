using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace RoomHub.Api.Infrastructure.Persistence;

// SQLite stores dates as text, so EF would read them back with Kind = Unspecified.
// This marks every value read from the database as UTC.
public sealed class UtcDateTimeConverter()
    : ValueConverter<DateTime, DateTime>(
        value => value,
        value => DateTime.SpecifyKind(value, DateTimeKind.Utc));
