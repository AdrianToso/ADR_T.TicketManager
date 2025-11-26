using Xunit;
using System;
using ADR_T.TicketManager.Core.Domain.Entities;

namespace ADR_T.TicketManager.Tests.Core.Domain.Entities;

public class TicketCommentTests
{
    private readonly Guid _validTicketId = Guid.NewGuid();
    private readonly Guid _validAutorId = Guid.NewGuid();
    private const string _validComentario = "Este es un comentario válido.";

    [Fact]
    public void Constructor_ShouldCreateInstance_WhenArgumentsAreValid()
    {
        var comment = new TicketComment(_validComentario, _validTicketId, _validAutorId);

        Assert.NotNull(comment);
        Assert.Equal(_validComentario, comment.Comentario);
        Assert.Equal(_validTicketId, comment.TicketId);
        Assert.Equal(_validAutorId, comment.AutorId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Constructor_ShouldThrowArgumentException_WhenComentarioIsNullOrEmpty(string? invalidComentario)
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            new TicketComment(invalidComentario, _validTicketId, _validAutorId));

        Assert.Contains("El comentario no puede estar vacío.", exception.Message);
        Assert.Equal("comentario", exception.ParamName);
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentException_WhenComentarioIsWhiteSpace()
    {
        const string whiteSpaceComentario = "   ";

        var exception = Assert.Throws<ArgumentException>(() =>
            new TicketComment(whiteSpaceComentario, _validTicketId, _validAutorId));

        Assert.Contains("El comentario no puede estar vacío.", exception.Message);
        Assert.Equal("comentario", exception.ParamName);
    }
}