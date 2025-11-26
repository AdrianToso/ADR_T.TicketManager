using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Xunit;

namespace ADR_T.TicketManager.IntegrationTests.Infrastructure;

public class RabbitMqFixture : IAsyncLifetime
{
    public IContainer Container { get; private set; } = default!;
    public string Hostname => "localhost";
    public int Port => Container.GetMappedPublicPort(5672);
    public string Username => "guest";
    public string Password => "guest";
    public string VirtualHost => "/";

    public async Task InitializeAsync()
    {
        Container = new ContainerBuilder()
            .WithImage("rabbitmq:3.13-management")
            .WithName($"rabbitmq-tests-{Guid.NewGuid()}")
            .WithPortBinding(5672, true)
            .WithPortBinding(15672, true)
            .WithEnvironment("RABBITMQ_DEFAULT_USER", Username)
            .WithEnvironment("RABBITMQ_DEFAULT_PASS", Password)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5672))
            .Build();

        await Container.StartAsync();
    }

    public async Task DisposeAsync()
    {
        if (Container is not null)
        {
            await Container.StopAsync();
            await Container.DisposeAsync();
        }
    }
}
