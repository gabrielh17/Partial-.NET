using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RoomHub.Api.Domain.Bookings;
using RoomHub.Api.Domain.Rooms;

namespace RoomHub.Api.Infrastructure.Persistence.Configurations;

public sealed class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.ToTable("Bookings");
        builder.HasKey(b => b.Id);
        // The Id is generated in the domain, not in the database.
        builder.Property(b => b.Id).ValueGeneratedNever();

        builder.Property(b => b.BookedBy).IsRequired();
        builder.Property(b => b.Attendees).IsRequired();

        // Enums are saved as text, so the database stays readable.
        builder.Property(b => b.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        // DateRange lives in the Bookings table (no separate table).
        builder.ComplexProperty(b => b.Range, range =>
        {
            range.Property(r => r.StartUtc)
                .HasColumnName("StartUtc")
                .HasConversion<UtcDateTimeConverter>();
            range.Property(r => r.EndUtc)
                .HasColumnName("EndUtc")
                .HasConversion<UtcDateTimeConverter>();
        });

        builder.HasOne<Room>()
            .WithMany()
            .HasForeignKey(b => b.RoomId)
            .OnDelete(DeleteBehavior.Restrict);

        // The overlap check (R7) looks up the active bookings of one room.
        builder.HasIndex(b => new { b.RoomId, b.Status });

        // The daily limit (R9) looks up the active bookings of one participant.
        builder.HasIndex(b => new { b.BookedBy, b.Status });

        // Computed from Status, not stored.
        builder.Ignore(b => b.IsActive);
    }
}
