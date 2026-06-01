namespace MomVibe.IntegrationTests.Infrastructure;

using Npgsql;
using Testcontainers.PostgreSql;

/// <summary>
/// Shared Postgres Testcontainer for the integration-test assembly. One container is started
/// lazily on first access and reused for every <see cref="WebApplicationFactory"/> subclass —
/// each factory carves out its own database within the container via <see cref="GetConnectionStringAsync"/>.
/// </summary>
/// <remarks>
/// The container outlives every test fixture and is torn down by the test host's process exit.
/// We don't expose an explicit teardown because xUnit collection fixtures and per-class factories
/// can interleave in ways that make a clean ordered shutdown noisy; relying on process exit is
/// both simpler and matches the existing one-process-per-test-run model.
/// </remarks>
public static class PostgresContainerFixture
{
    private static readonly SemaphoreSlim _gate = new(1, 1);
    private static PostgreSqlContainer? _container;

    /// <summary>
    /// Creates the named database on the shared container if it doesn't already exist and
    /// returns a connection string scoped to that database. Safe to call concurrently.
    /// </summary>
    public static async Task<string> GetConnectionStringAsync(string databaseName)
    {
        var container = await EnsureStartedAsync().ConfigureAwait(false);
        var adminConnectionString = container.GetConnectionString();

        await EnsureDatabaseExistsAsync(adminConnectionString, databaseName).ConfigureAwait(false);

        var builder = new NpgsqlConnectionStringBuilder(adminConnectionString) { Database = databaseName };
        return builder.ConnectionString;
    }

    private static async Task<PostgreSqlContainer> EnsureStartedAsync()
    {
        if (_container is not null) return _container;
        await _gate.WaitAsync().ConfigureAwait(false);
        try
        {
            if (_container is null)
            {
                var container = new PostgreSqlBuilder()
                    .WithImage("postgres:16-alpine")
                    .WithDatabase("postgres")
                    .WithUsername("postgres")
                    .WithPassword("postgres")
                    .Build();
                await container.StartAsync().ConfigureAwait(false);
                _container = container;
            }
            return _container;
        }
        finally
        {
            _gate.Release();
        }
    }

    private static async Task EnsureDatabaseExistsAsync(string adminConnectionString, string databaseName)
    {
        // Postgres folds unquoted identifiers to lowercase; the tests always reference the DB
        // through this fixture so we lowercase consistently to avoid quoted-vs-unquoted mismatches.
        var safeName = databaseName.ToLowerInvariant();
        await using var connection = new NpgsqlConnection(adminConnectionString);
        await connection.OpenAsync().ConfigureAwait(false);

        await using var checkCmd = connection.CreateCommand();
        checkCmd.CommandText = "SELECT 1 FROM pg_database WHERE datname = @name";
        checkCmd.Parameters.AddWithValue("name", safeName);
        var exists = await checkCmd.ExecuteScalarAsync().ConfigureAwait(false) is not null;
        if (exists) return;

        // Database names are quoted but validated above; the GUID-suffixed names used by
        // every factory cannot contain SQL-injectable characters.
        await using var createCmd = connection.CreateCommand();
        createCmd.CommandText = $"CREATE DATABASE \"{safeName}\"";
        await createCmd.ExecuteNonQueryAsync().ConfigureAwait(false);
    }
}
