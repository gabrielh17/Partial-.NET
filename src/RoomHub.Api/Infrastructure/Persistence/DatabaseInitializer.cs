using Microsoft.EntityFrameworkCore;

namespace RoomHub.Api.Infrastructure.Persistence;

public static class DatabaseInitializer
{
    // With migrations: apply them. Without migrations (fresh clone): create the schema,
    // so the app works right away. Existing data is kept in both cases.
    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<RoomHubDbContext>();

        if (db.Database.GetMigrations().Any())
            await db.Database.MigrateAsync();
        else
            await db.Database.EnsureCreatedAsync();
    }
}
