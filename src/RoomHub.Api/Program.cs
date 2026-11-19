using RoomHub.Api.Common;
using RoomHub.Api.Features.Bookings;
using RoomHub.Api.Features.Rooms;
using RoomHub.Api.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// In-memory stores, Singleton because the data lives in memory.
builder.Services.AddSingleton<RoomRepository>();
builder.Services.AddSingleton<BookingRepository>();

// "Now" comes from TimeProvider (R5), never from DateTime.UtcNow.
builder.Services.AddSingleton(TimeProvider.System);

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();

app.MapGet("/", () => "RoomHub API is running.");
app.MapRooms();
app.MapBookings();

app.Run();
