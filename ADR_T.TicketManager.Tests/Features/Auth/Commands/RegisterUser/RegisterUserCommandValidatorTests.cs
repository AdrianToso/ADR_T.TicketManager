using Xunit;
using FluentValidation.TestHelper;
using System;
using ADR_T.TicketManager.Application.Features.Auth.Commands.RegisterUser;
using FluentValidation;
using MediatR;

namespace ADR_T.TicketManager.Tests.Application.Features.Auth.Commands;

public class RegisterUserCommandValidatorTests
{
    private readonly RegisterUserCommandValidator _validator;

    // ARRANGE
    private readonly RegisterUserCommand _validCommand = new(
        Email: "usuario.valido@ejemplo.com",
        Password: "PasswordSeguro123"
    );

    public RegisterUserCommandValidatorTests()
    {
        // ARRANGE
        _validator = new RegisterUserCommandValidator();
    }

    [Fact]
    public void Should_Have_Error_When_Email_Is_Empty()
    {
        // ARRANGE
        var command = _validCommand with { Email = string.Empty };

        // ACT & ASSERT
        _validator.TestValidate(command)
            .ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email es requerido.");
    }

    [Fact]
    public void Should_Have_Error_When_Email_Format_Is_Invalid()
    {
        // ARRANGE
        var command = _validCommand with { Email = "email_invalido" };

        // ACT & ASSERT
        _validator.TestValidate(command)
            .ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Formato de mail invalido");
    }


    [Fact]
    public void Should_Have_Error_When_Password_Is_Empty()
    {
        // ARRANGE
        var command = _validCommand with { Password = string.Empty };

        // ACT & ASSERT
        _validator.TestValidate(command)
            .ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password es requerido.");
    }

    [Fact]
    public void Should_Have_Error_When_Password_Is_Too_Short()
    {
        // ARRANGE
        var command = _validCommand with { Password = "shortp7" };

        // ACT & ASSERT
        _validator.TestValidate(command)
            .ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password debe tener 8 caracteres como minimo.");
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