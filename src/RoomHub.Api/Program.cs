using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using RoomHub.Api.Common.Cqrs;
using RoomHub.Api.Domain.Bookings;
using RoomHub.Api.Domain.Common;
using RoomHub.Api.Domain.Rooms;
using RoomHub.Api.Features.Bookings;
using RoomHub.Api.Features.Rooms;
using RoomHub.Api.Infrastructure.Persistence;
using RoomHub.Api.Infrastructure.Persistence.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Enums travel as text in JSON ("Active", "Cancelled").
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// "Now" comes from TimeProvider (R5), never from the system clock directly.
builder.Services.AddSingleton(TimeProvider.System);

// The DbContext is Scoped: a new instance for each HTTP request.
builder.Services.AddDbContext<RoomHubDbContext>((serviceProvider, options) =>
    options.UseSqlite(serviceProvider
        .GetRequiredService<IConfiguration>()
        .GetConnectionString("RoomHub")));

// Repositories and the Unit of Work are Scoped and share the same DbContext per request.
builder.Services.AddScoped<IUnitOfWork>(serviceProvider =>
    serviceProvider.GetRequiredService<RoomHubDbContext>());
builder.Services.AddScoped<IRoomRepository, RoomRepository>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();

// Every ICommandHandler in this assembly, found once at startup.
builder.Services.AddHandlers(typeof(Program).Assembly);

var app = builder.Build();

await app.InitializeDatabaseAsync();

app.MapGet("/", () => "RoomHub API is running.");
app.MapRooms();
app.MapBookings();

app.Run();
