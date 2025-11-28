using Testcontainers.MsSql;
using DotNet.Testcontainers.Builders;
using Xunit;

namespace ADR_T.TicketManager.IntegrationTests.Infrastructure;

public sealed class TestingDatabase : IAsyncLifetime
{
    private readonly MsSqlContainer _container;

    public string ConnectionString => _container.GetConnectionString();

    public TestingDatabase()
    {
        _container = new MsSqlBuilder()
            .WithPassword("YourStrong(!)Password")
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}
