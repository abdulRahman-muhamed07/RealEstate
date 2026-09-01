namespace RealEstate.Application.Common;

public sealed record ServiceResult<T>(bool Success, T? Data, string? Error, int StatusCode = 200)
{
    public static ServiceResult<T> Ok(T data) => new(true, data, null, 200);
    public static ServiceResult<T> Fail(string error, int statusCode) => new(false, default, error, statusCode);
}
