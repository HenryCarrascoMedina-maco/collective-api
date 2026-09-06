namespace Collective.Api.Common.Results;

/// <summary>Tipo de fallo esperado. Determina el codigo HTTP en el endpoint.</summary>
public enum ErrorType
{
    Validation,
    NotFound,
    Conflict,
    Unexpected,
}

/// <summary>
/// Fallo esperado del dominio. No es una excepcion: es una respuesta normal del
/// sistema que viaja dentro de un <see cref="Result{T}"/> (regla A6).
/// </summary>
public sealed record Error(string Code, string Message, ErrorType Type)
{
    public static Error Validation(string code, string message) => new(code, message, ErrorType.Validation);

    public static Error NotFound(string code, string message) => new(code, message, ErrorType.NotFound);

    public static Error Conflict(string code, string message) => new(code, message, ErrorType.Conflict);
}
