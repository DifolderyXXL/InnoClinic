namespace MicroserviceApiKernel.Results;

public class Result
{
    public Result(bool isSuccess, Error? exception)
    {
        IsSuccess = isSuccess;
        Error = exception;
    }

    public bool IsSuccess { get; }
    public bool IsError => Error != null;
    public Error? Error { get; }


    public static Result Success() => new(true, null);

    public static Result<TValue> Success<TValue>(TValue value) =>
        new(value, true, null);

    public static Result Failure(Error? error) => new(false, error);

    public static Result<TValue> Failure<TValue>(Error? error) =>
        new(default, false, error);


    public static implicit operator Result(Error error)
        => Failure(error);


}

public class Result<T> : Result
{
    public Result(T? value, bool isSuccess, Error? exception) : base(isSuccess, exception)
    {
        Value = value;
    }

    public T? Value { get; }


    public static implicit operator Result<T>(T? value)
        => value is not null ? Success(value) : Failure<T>(null);


    public static implicit operator Result<T>(Error error)
        => Failure<T>(error);

}
