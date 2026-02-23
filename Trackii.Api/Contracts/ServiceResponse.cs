namespace Trackii.Api.Contracts;

public enum ServiceErrorType
{
    BadRequest,
    Unauthorized,
    Conflict,
    NotFound
}

public sealed class ServiceResponse<T>
{
    public bool Success { get; init; }
    public T? Data { get; init; }
    public string? Message { get; init; }
    public ServiceErrorType? ErrorType { get; init; }

    public static ServiceResponse<T> Ok(T data) => new() { Success = true, Data = data };

    public static ServiceResponse<T> Fail(string message, ServiceErrorType errorType = ServiceErrorType.BadRequest) => new()
    {
        Success = false,
        Message = message,
        ErrorType = errorType
    };
}
