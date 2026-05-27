using Application.Abstractions.Authentication;
using Domain.Users;
using Infrastructure.Database;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Respawn;
using Respawn.Graph;
using Testcontainers.PostgreSql;
using Web.Api;

namespace IntegrationTests;

public sealed class CrowdNfoApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string AdminUsername = "admin";
    public const string AdminPassword = "admin-password-0123456789";

    private const string JwtSecret = "integration-tests-signing-secret-key-0123456789";

    private readonly PostgreSqlContainer _database = new PostgreSqlBuilder()
        .WithImage("postgres:17")
        .Build();

    private Respawner _respawner = null!;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // "Testing" skips appsettings.Development.json (Host=postgres) and Program's dev-only migration.
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:Database", _database.GetConnectionString());
        builder.UseSetting("Jwt:Secret", JwtSecret);
        builder.UseSetting("Jwt:Issuer", "crowdnfo-api");
        builder.UseSetting("Jwt:Audience", "crowdnfo-api");
    }

    public async Task ResetDatabaseAsync()
    {
        await using NpgsqlConnection connection = new(_database.GetConnectionString());
        await connection.OpenAsync();
        await _respawner.ResetAsync(connection);

        await SeedAdminAsync();
    }

    async Task IAsyncLifetime.InitializeAsync()
    {
        await _database.StartAsync();

        using (IServiceScope scope = Services.CreateScope())
        {
            ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await dbContext.Database.MigrateAsync();
        }

        await using NpgsqlConnection connection = new(_database.GetConnectionString());
        await connection.OpenAsync();
        _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = ["public"],
            TablesToIgnore = [new Table("public", "roles"), new Table("public", "__EFMigrationsHistory")]
        });
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _database.DisposeAsync();
        await base.DisposeAsync();
    }

    private async Task SeedAdminAsync()
    {
        using IServiceScope scope = Services.CreateScope();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        IPasswordHasher passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        Role adminRole = await dbContext.Roles.SingleAsync(role => role.Name == RoleNames.Admin);

        var admin = User.Register(AdminUsername, "admin@crowdnfo.test", passwordHasher.Hash(AdminPassword), adminRole.Id);
        admin.Activate();

        dbContext.Users.Add(admin);
        await dbContext.SaveChangesAsync();
    }
}
