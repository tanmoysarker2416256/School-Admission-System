using Microsoft.EntityFrameworkCore;
using SchoolAdmission.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace SchoolAdmission.Tests;

public class PostgresFixture : IAsyncLifetime
{
    private PostgreSqlContainer? _container;

    public string ConnectionString { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        // optional: point the tests at your own EMPTY test database instead of Docker
        var external = Environment.GetEnvironmentVariable("TEST_DB_CONNECTION");

        if (!string.IsNullOrWhiteSpace(external))
        {
            ConnectionString = external;
        }
        else
        {
            _container = new PostgreSqlBuilder().WithImage("postgres:16-alpine").Build();
            await _container.StartAsync();
            ConnectionString = _container.GetConnectionString();
        }

        await using var db = CreateDb();
        await db.Database.MigrateAsync();   // builds the real schema, including the unique index
    }

    public async Task DisposeAsync()
    {
        if (_container is not null)
            await _container.DisposeAsync();
    }

    public SchoolAdmissionDbContext CreateDb() =>
        new(new DbContextOptionsBuilder<SchoolAdmissionDbContext>().UseNpgsql(ConnectionString).Options);
}

[CollectionDefinition("db")]
public class DbCollection : ICollectionFixture<PostgresFixture>;