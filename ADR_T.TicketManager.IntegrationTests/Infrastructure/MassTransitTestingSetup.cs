using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ADR_T.TicketManager.IntegrationTests.Infrastructure;

public class MassTransitTestingSetup<TEntryPoint> : WebApplicationFactory<TEntryPoint>
    where TEntryPoint : class
{
    private readonly RabbitMqFixture _rabbit;

    public MassTransitTestingSetup(RabbitMqFixture rabbit)
    {
        _rabbit = rabbit;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.AddMassTransit(x =>
            {
                x.UsingRabbitMq((context, cfg) =>
                {
                    var uri = new Uri($"rabbitmq://{_rabbit.Hostname}:{_rabbit.Port}/{_rabbit.VirtualHost}");

                    cfg.Host(uri, h =>
                    {
                        h.Username(_rabbit.Username);
                        h.Password(_rabbit.Password);
                    });
                });
            });
        });
    }
}
