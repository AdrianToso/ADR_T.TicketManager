using Xunit;
using FluentValidation.TestHelper;
using System;
using ADR_T.TicketManager.Application.Features.Users.Queries.GetUserById;
using FluentValidation;

namespace ADR_T.TicketManager.Tests.Application.Features.Users.Queries;

public class GetUserByIdQueryValidatorTests
{
    private readonly GetUserByIdQueryValidator _validator;

    public GetUserByIdQueryValidatorTests()
    {
        // ARRANGE
        _validator = new GetUserByIdQueryValidator();
    }

    [Fact]
    public void Should_Have_Error_When_UserId_Is_Empty()
    {
        // ARRANGE
        var query = new GetUserByIdQuery(Guid.Empty);

        // ACT
        var result = _validator.TestValidate(query);

        // ASSERT
        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage("El ID del Usuario es obligatorio.");
    }

    [Fact]
    public void Should_Not_Have_Error_When_UserId_Is_Valid()
    {
        // ARRANGE
        var query = new GetUserByIdQuery(Guid.NewGuid());

        // ACT
        var result = _validator.TestValidate(query);

        // ASSERT
        result.ShouldNotHaveValidationErrorFor(x => x.UserId);
    }
}