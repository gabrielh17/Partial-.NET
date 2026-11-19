namespace RoomHub.Api.Domain.Common;

// Success or business failure. Exceptions stay for unexpected situations
// (a bug, the database being down), not for "the room is occupied".
public sealed class Result<T>
{
    private readonly T? _value;
    private readonly Error? _error;

    private Result(T? value, Error? error)
    {
        _value = value;
        _error = error;
    }

    public bool IsSuccess => _error is null;
    public bool IsFailure => !IsSuccess;

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot read the value of a failed result.");

    public Error Error => IsFailure
        ? _error!
        : throw new InvalidOperationException("Cannot read the error of a successful result.");

    public static Result<T> Success(T value) => new(value, null);
    public static Result<T> Failure(Error error) => new(default, error);

    // Lets a method write "return booking;" or "return BookingErrors.X;".
    public static implicit operator Result<T>(T value) => Success(value);
    public static implicit operator Result<T>(Error error) => Failure(error);
}
