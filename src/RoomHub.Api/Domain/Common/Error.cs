namespace RoomHub.Api.Domain.Common;

public enum ErrorType
{
    Validation,
    NotFound,
    Conflict
}

// A business error as a value. Code is stable for clients; Type is mapped
// to an HTTP status in one place (Common/ErrorHttpExtensions.ToProblem).
public sealed record Error(string Code, string Message, ErrorType Type)
{
    public static Error Validation(string code, string message) =>
        new(code, message, ErrorType.Validation);

    public static Error NotFound(string code, string message) =>
        new(code, message, ErrorType.NotFound);

    public static Error Conflict(string code, string message) =>
        new(code, message, ErrorType.Conflict);
}
