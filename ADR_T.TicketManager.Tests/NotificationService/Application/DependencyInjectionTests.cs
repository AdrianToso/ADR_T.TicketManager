using Microsoft.Extensions.DependencyInjection;
using ADR_T.NotificationService.Application;
namespace ADR_T.TicketManager.Tests.NotificationService.Application;

public class DependencyInjectionTests
{
    [Fact]
    public void AddApplicationServices_ShouldReturnIServiceCollection()
    {
        // Arrange
        IServiceCollection services = new ServiceCollection();

        // Act
        var result = services.AddApplicationServices();

        // Assert
        Assert.NotNull(result);
        Assert.Same(services, result);

    }
}