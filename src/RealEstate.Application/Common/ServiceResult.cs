namespace RealEstate.Application.Common;

public enum ErrorCode
{
    None,
    Validation,
    Unauthorized,
    Forbidden,
    NotFound,
    Conflict,
    InvalidOperation,
    Unexpected
}

public sealed record Result<T>(bool Success, T? Data, ErrorCode ErrorCode = ErrorCode.None, string? Error = null)
{
    public static Result<T> Ok(T data) => new(true, data);
    public static Result<T> Fail(ErrorCode code, string message) => new(false, default, code, message);
}
