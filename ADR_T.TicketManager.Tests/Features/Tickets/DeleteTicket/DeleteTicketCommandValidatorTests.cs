using Xunit;
using FluentValidation.TestHelper;
using System;
using ADR_T.TicketManager.Application.Features.Tickets.Commands.DeleteTicket;
using FluentValidation;

namespace ADR_T.TicketManager.Tests.Application.Features.Tickets.Commands;

public class DeleteTicketCommandValidatorTests
{
    private readonly DeleteTicketCommandValidator _validator;

    public DeleteTicketCommandValidatorTests()
    {
        // ARRANGE
        _validator = new DeleteTicketCommandValidator();
    }

    [Fact]
    public void Should_Have_Error_When_TicketId_Is_Empty()
    {
        // ARRANGE
        var command = new DeleteTicketCommand(Guid.Empty);

        // ACT & ASSERT
        _validator.TestValidate(command)
            .ShouldHaveValidationErrorFor(x => x.TicketId)
            .WithErrorMessage("El ID es obligatorio.");
    }

    [Fact]
    public void Should_Not_Have_Error_When_TicketId_Is_Valid()
    {
        // ARRANGE
        var command = new DeleteTicketCommand(Guid.NewGuid());

        // ACT & ASSERT
        _validator.TestValidate(command).ShouldNotHaveAnyValidationErrors();
    }
}