using Xunit;
using Microsoft.Extensions.DependencyInjection;
using System;
using ADR_T.TicketManager.Application;
using AutoMapper;
using MediatR;
using FluentValidation; 
using ADR_T.TicketManager.Application.Features.Auth.Commands.LoginUser; 

namespace ADR_T.TicketManager.Tests.Application;

public class DependencyInjectionTests
{
    [Fact]
    public void AddApplication_ShouldRegisterAllRequiredServices()
    {
        // ARRANGE
        var services = new ServiceCollection();

        // ACT
        var serviceProvider = services.AddApplication().BuildServiceProvider();

        // ASSERT

        Assert.NotNull(serviceProvider.GetService<IMapper>());

        Assert.NotNull(serviceProvider.GetService<IMediator>());

        var loginValidator = serviceProvider.GetService<IValidator<LoginUserCommand>>();
        Assert.NotNull(loginValidator); 
        
    }
}