namespace ADR_T.TicketManager.Application.Common.Models;
/// <summary>
/// Representa el resultado de una operación que puede o no tener éxito,
/// y puede contener un valor y una lista de errores.
/// </summary>
public class Result<T> : Result
{
    private Result(bool succeeded, T data, IEnumerable<string> errors) : base(succeeded, errors)
    {
        Data = data;
    }

    /// <summary>
    /// El valor de la operación si fue exitosa.
    /// </summary>
    public T Data { get; }

    /// <summary>
    /// Crea un resultado exitoso con datos.
    /// </summary>
    public static Result<T> Success(T data)
    {
        return new Result<T>(true, data, Enumerable.Empty<string>());
    }

    /// <summary>
    /// Crea un resultado fallido con una lista de errores.
    /// </summary>
    public static new Result<T> Failure(IEnumerable<string> errors)
    {
        return new Result<T>(false, default(T), errors);
    }
}

/// <summary>
/// Representa el resultado de una operación que puede o no tener éxito,
/// sin un valor de retorno específico, pero con una lista de errores opcional.
/// </summary>
public class Result
{
    protected Result(bool succeeded, IEnumerable<string> errors)
    {
        Succeeded = succeeded;
        Errors = errors.ToList();
    }

    /// <summary>
    /// Indica si la operación fue exitosa.
    /// </summary>
    public bool Succeeded { get; }

    /// <summary>
    /// Lista de errores si la operación falló.
    /// </summary>
    public List<string> Errors { get; }

    /// <summary>
    /// Crea un resultado exitoso sin errores ni datos.
    /// </summary>
    public static Result Success()
    {
        return new Result(true, Enumerable.Empty<string>());
    }

    /// <summary>
    /// Crea un resultado fallido con una lista de errores.
    /// </summary>
    public static Result Failure(IEnumerable<string> errors)
    {
        return new Result(false, errors);
    }

    /// <summary>
    /// Crea un resultado fallido con un único error.
    /// </summary>
    public static Result Failure(string error)
    {
        return new Result(false, new List<string> { error });
    }
}