using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ApiCrmAlive.Context;

// Design-time factory so `dotnet ef` doesn't need to bootstrap the full WebApplication.
public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    private static void LoadEnvFromLikelyLocations()
    {
        var cwd = Directory.GetCurrentDirectory();
        var direct = Path.Combine(cwd, ".env");
        if (File.Exists(direct)) { Env.Load(direct); return; }

        var probe = cwd;
        for (var i = 0; i < 5; i++)
        {
            probe = Directory.GetParent(probe)?.FullName ?? probe;
            var candidate = Path.Combine(probe, ".env");
            if (File.Exists(candidate)) { Env.Load(candidate); return; }
        }

        try { Env.Load(); } catch { /* ignore */ }
    }

    public AppDbContext CreateDbContext(string[] args)
    {
        LoadEnvFromLikelyLocations();

        // Prefer explicit connection string if provided (works well for local dev to control SSL mode).
        var cs =
            Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection") ??
            Environment.GetEnvironmentVariable("DefaultConnection");

        if (string.IsNullOrWhiteSpace(cs))
        {
            var dbHost = Environment.GetEnvironmentVariable("DB_HOST");
            var dbPort = Environment.GetEnvironmentVariable("DB_PORT") ?? "5432";
            var dbName = Environment.GetEnvironmentVariable("DB_NAME");
            var dbUser = Environment.GetEnvironmentVariable("DB_USER");
            var dbPass = Environment.GetEnvironmentVariable("DB_PASSWORD");

            if (string.IsNullOrWhiteSpace(dbHost) ||
                string.IsNullOrWhiteSpace(dbName) ||
                string.IsNullOrWhiteSpace(dbUser) ||
                string.IsNullOrWhiteSpace(dbPass))
            {
                throw new InvalidOperationException(
                    "Para rodar migrations, defina ConnectionStrings__DefaultConnection ou as variáveis DB_HOST/DB_NAME/DB_USER/DB_PASSWORD no .env.");
            }

            // Keep consistent with Program.cs default behavior.
            cs =
                $"Host={dbHost};Port={dbPort};Database={dbName};Username={dbUser};Password={dbPass};Ssl Mode=Require;Trust Server Certificate=true";
        }

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(cs, npgsql =>
            {
                npgsql.CommandTimeout(120);
                npgsql.EnableRetryOnFailure();
            })
            .Options;

        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        return new AppDbContext(options);
    }
}

