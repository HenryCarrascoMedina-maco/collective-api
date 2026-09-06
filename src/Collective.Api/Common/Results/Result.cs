namespace Collective.Api.Common.Results;

/// <summary>Resultado de un caso de uso sin valor de retorno.</summary>
public class Result
{
    protected Result(bool isSuccess, Error? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public Error? Error { get; }

    public static Result Success() => new(true, null);

    public static Result<TValue> Success<TValue>(TValue value) => Result<TValue>.Success(value);

    public static Result Failure(Error error) => new(false, error);

    public static Result<TValue> Failure<TValue>(Error error) => Result<TValue>.Failure(error);
}

/// <summary>Resultado de un caso de uso que devuelve un valor.</summary>
public sealed class Result<TValue> : Result
{
    private readonly TValue? _value;

    private Result(bool isSuccess, TValue? value, Error? error)
        : base(isSuccess, error) => _value = value;

    /// <summary>Valor del resultado. Solo valido cuando <see cref="Result.IsSuccess"/>.</summary>
    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("A failed result has no value.");

    public static Result<TValue> Success(TValue value) => new(true, value, null);

    public static new Result<TValue> Failure(Error error) => new(false, default, error);
}
