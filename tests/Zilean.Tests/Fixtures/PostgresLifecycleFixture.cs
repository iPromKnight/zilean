namespace Zilean.Tests.Fixtures;

public class PostgresLifecycleFixture : IAsyncLifetime
{
    private PostgreSqlContainer PostgresContainer { get; } = new PostgreSqlBuilder()
        .WithImage("postgres:16.3-alpine3.20")
        .WithPortBinding(5432, 5432)
        .WithEnvironment("POSTGRES_HOST", "postgres")
        .WithEnvironment("POSTGRES_DB", "zilean")
        .WithEnvironment("POSTGRES_USER", "postgres")
        .WithEnvironment("POSTGRES_PASSWORD", "postgres")
        .Build();

    public ZileanConfiguration ZileanConfiguration { get; } = new();

    public PostgresLifecycleFixture() =>
        DerivePathInfo(
            (_, projectDirectory, type, method) => new(
                directory: Path.Combine(projectDirectory, "Verification"),
                typeName: type.Name,
                methodName: method.Name));

    public async Task InitializeAsync()
    {
        await PostgresContainer.StartAsync();
        ZileanConfiguration.Database.ConnectionString = PostgresContainer.GetConnectionString();
    }

    public Task DisposeAsync() => PostgresContainer.DisposeAsync().AsTask();
}
