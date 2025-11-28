namespace ADR_T.TicketManager.Core.Domain.Exceptions;
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}
