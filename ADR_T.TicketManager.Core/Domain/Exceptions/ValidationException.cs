namespace ADR_T.TicketManager.Core.Domain.Exceptions;
public class ValidationException: Exception
{
    public ValidationException(string message) : base(message) { }
}
