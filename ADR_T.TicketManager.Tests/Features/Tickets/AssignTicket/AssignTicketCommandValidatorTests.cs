using Xunit;
using FluentValidation.TestHelper;
using System;
using ADR_T.TicketManager.Application.Features.Tickets.Commands.AssignTicket;
using FluentValidation;
using MediatR;


namespace ADR_T.TicketManager.Tests.Application.Features.Tickets.Commands;

public class AssignTicketCommandValidatorTests
{
    private readonly AssignTicketCommandValidator _validator;

    // ARRANGE
    private readonly AssignTicketCommand _validCommand = new(
        TicketId: Guid.NewGuid(),
        TecnicoId: Guid.NewGuid(),
        AsignadorUserId: Guid.NewGuid()
    );

    public AssignTicketCommandValidatorTests()
    {
        // ARRANGE
        _validator = new AssignTicketCommandValidator();
    }

    [Fact]
    public void Should_Have_Error_When_TicketId_Is_Empty()
    {
        // ARRANGE
        var command = _validCommand with { TicketId = Guid.Empty };

        // ACT & ASSERT
        _validator.TestValidate(command)
            .ShouldHaveValidationErrorFor(x => x.TicketId)
            .WithErrorMessage("El ID del Ticket es obligatorio.");
    }

    [Fact]
    public void Should_Have_Error_When_TecnicoId_Is_Empty()
    {
        // ARRANGE
        var command = _validCommand with { TecnicoId = Guid.Empty };

        // ACT & ASSERT
        _validator.TestValidate(command)
            .ShouldHaveValidationErrorFor(x => x.TecnicoId)
            .WithErrorMessage("El ID del Técnico es obligatorio.");
    }

    [Fact]
    public void Should_Have_Error_When_AsignadorUserId_Is_Empty()
    {
        // ARRANGE
        var command = _validCommand with { AsignadorUserId = Guid.Empty };

        // ACT & ASSERT
        _validator.TestValidate(command)
            .ShouldHaveValidationErrorFor(x => x.AsignadorUserId)
            .WithErrorMessage("El ID del usuario que asigna es obligatorio.");
    }

    [Fact]
    public void Should_Not_Have_Any_Validation_Errors_When_Command_Is_Valid()
    {
        // ARRANGE
        var command = _validCommand;

        // ACT & ASSERT
        _validator.TestValidate(command).ShouldNotHaveAnyValidationErrors();
    }
}