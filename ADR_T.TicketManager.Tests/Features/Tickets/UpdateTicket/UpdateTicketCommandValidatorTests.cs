using Xunit;
using FluentValidation.TestHelper;
using System;
using ADR_T.TicketManager.Application.Features.Tickets.Commands.UpdateTicket;
using ADR_T.TicketManager.Core.Domain.Enums;
using FluentValidation;

namespace ADR_T.TicketManager.Tests.Application.Features.Tickets.Commands;

public class UpdateTicketCommandValidatorTests
{
    private readonly UpdateTicketCommandValidator _validator;

    // ARRANGE
    private readonly UpdateTicketCommand _validCommand = new()
    {
        Id = Guid.NewGuid(),
        Titulo = "Título Válido",
        Descripcion = "Descripción válida y obligatoria.",
        Prioridad = TicketPriority.Alta,
        Status = TicketStatus.Abierto,
        CreadoByUserId = Guid.NewGuid() 
    };

    public UpdateTicketCommandValidatorTests()
    {
        // ARRANGE
        _validator = new UpdateTicketCommandValidator();
    }


    [Fact]
    public void Should_Have_Error_When_Titulo_Is_Empty()
    {
        // ARRANGE
        var command = new UpdateTicketCommand
        {
            Id = _validCommand.Id,
            Titulo = string.Empty, 
            Descripcion = _validCommand.Descripcion,
            Prioridad = _validCommand.Prioridad,
            Status = _validCommand.Status,
            CreadoByUserId = _validCommand.CreadoByUserId
        };

        // ACT & ASSERT
        _validator.TestValidate(command)
            .ShouldHaveValidationErrorFor(x => x.Titulo)
            .WithErrorMessage("El Titulo es obligatorio.");
    }

    [Fact]
    public void Should_Have_Error_When_Titulo_Exceeds_100_Characters()
    {
        // ARRANGE
        var longTitulo = new string('A', 101);
        var command = new UpdateTicketCommand
        {
            Id = _validCommand.Id,
            Titulo = longTitulo, 
            Descripcion = _validCommand.Descripcion,
            Prioridad = _validCommand.Prioridad,
            Status = _validCommand.Status,
            CreadoByUserId = _validCommand.CreadoByUserId
        };

        // ACT & ASSERT
        _validator.TestValidate(command)
            .ShouldHaveValidationErrorFor(x => x.Titulo)
            .WithErrorMessage("Máximo 100 caracteres.");
    }

    [Fact]
    public void Should_Have_Error_When_Descripcion_Is_Empty()
    {
        // ARRANGE
        var command = new UpdateTicketCommand
        {
            Id = _validCommand.Id,
            Titulo = _validCommand.Titulo,
            Descripcion = string.Empty,
            Prioridad = _validCommand.Prioridad,
            Status = _validCommand.Status,
            CreadoByUserId = _validCommand.CreadoByUserId
        };

        // ACT & ASSERT
        _validator.TestValidate(command)
            .ShouldHaveValidationErrorFor(x => x.Descripcion)
            .WithErrorMessage("La Descripcion es obligatoria.");
    }

    [Fact]
    public void Should_Have_Error_When_Prioridad_Is_Invalid_Enum_Value()
    {
        // ARRANGE
        var command = new UpdateTicketCommand
        {
            Id = _validCommand.Id,
            Titulo = _validCommand.Titulo,
            Descripcion = _validCommand.Descripcion,
            Prioridad = (TicketPriority)100, 
            Status = _validCommand.Status,
            CreadoByUserId = _validCommand.CreadoByUserId
        };

        // ACT & ASSERT
        _validator.TestValidate(command)
            .ShouldHaveValidationErrorFor(x => x.Prioridad)
            .WithErrorMessage("Prioridad no válida.");
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