using Xunit;
using FluentValidation.TestHelper;
using System;
using ADR_T.TicketManager.Application.Features.Tickets.Commands.CreateTicket;
using ADR_T.TicketManager.Core.Domain.Enums;
using FluentValidation;
using MediatR;

namespace ADR_T.TicketManager.Tests.Application.Features.Tickets.Commands;

public class CreateTicketCommandValidatorTests
{
    private readonly CreateTicketCommandValidator _validator;

    // ARRANGE
    private readonly CreateTicketCommand _validCommand = new(
        "Título Válido",
        "Descripción válida y obligatoria.",
        TicketPriority.Alta,
        Guid.NewGuid()
    );

    public CreateTicketCommandValidatorTests()
    {
        _validator = new CreateTicketCommandValidator();
    }

    [Fact]
    public void Should_Have_Error_When_Titulo_Is_Empty()
    {
        var command = new CreateTicketCommand(
            string.Empty,
            _validCommand.Descripcion,
            _validCommand.Prioridad,
            _validCommand.CreadoByUserId
        );

        _validator.TestValidate(command)
            .ShouldHaveValidationErrorFor(x => x.Titulo)
            .WithErrorMessage("El Titulo es obligatorio.");
    }

    [Fact]
    public void Should_Have_Error_When_Titulo_Exceeds_100_Characters()
    {
        var longTitulo = new string('A', 101);
        var command = new CreateTicketCommand(
            longTitulo,
            _validCommand.Descripcion,
            _validCommand.Prioridad,
            _validCommand.CreadoByUserId
        );

        _validator.TestValidate(command)
            .ShouldHaveValidationErrorFor(x => x.Titulo)
            .WithErrorMessage("Máximo 100 caracteres.");
    }

    [Fact]
    public void Should_Have_Error_When_Descripcion_Is_Empty()
    {
        var command = new CreateTicketCommand(
            _validCommand.Titulo,
            string.Empty,
            _validCommand.Prioridad,
            _validCommand.CreadoByUserId
        );

        _validator.TestValidate(command)
            .ShouldHaveValidationErrorFor(x => x.Descripcion)
            .WithErrorMessage("La Descripcion es obligatoria.");
    }

    [Fact]
    public void Should_Have_Error_When_CreadoByUserId_Is_Empty()
    {
        var command = new CreateTicketCommand(
            _validCommand.Titulo,
            _validCommand.Descripcion,
            _validCommand.Prioridad,
            Guid.Empty
        );

        _validator.TestValidate(command)
            .ShouldHaveValidationErrorFor(x => x.CreadoByUserId)
            .WithErrorCode("NotEmptyValidator");
    }

    [Fact]
    public void Should_Not_Have_Any_Validation_Errors_When_Command_Is_Valid()
    {
        var command = _validCommand;

        _validator.TestValidate(command).ShouldNotHaveAnyValidationErrors();
    }
}