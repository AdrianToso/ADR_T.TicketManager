using System;
using System.Runtime.Serialization;

namespace ADR_T.TicketManager.Core.Domain.Exceptions;

/// <summary>
/// Excepción personalizada para representar errores que ocurren durante operaciones de persistencia.
/// Envuelve excepciones específicas de la infraestructura subyacente.
/// </summary>
[Serializable]
public class PersistenceException : Exception
{
    public PersistenceException() : base() { }

    public PersistenceException(string message) : base(message) { }

    public PersistenceException(string message, Exception innerException) : base(message, innerException) { }

    // Constructor requerido para la serialización
    protected PersistenceException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}