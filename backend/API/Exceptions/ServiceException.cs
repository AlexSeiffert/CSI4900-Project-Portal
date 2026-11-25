namespace API.Exceptions;

public class ServiceException(string operation, ErrorType type, string message, Exception innerException) : Exception(message, innerException)
{
    public string Operation { get; } = operation;
    public ErrorType Type { get; } = type;

    public static ServiceException NotFound(string operation, string message)
    {
        return new(operation, ErrorType.NotFound, message, new KeyNotFoundException(message));
    }

    public static ServiceException Validation(string operation, string message)
    {
        return new(operation, ErrorType.Validation, message, new InvalidOperationException(message));
    }

    public static ServiceException Forbidden(string operation, string message)
    {
        return new(operation, ErrorType.Forbidden, message, new UnauthorizedAccessException(message));
    }

    public static ServiceException Unavailable(string operation, string message)
    {
        return new(operation, ErrorType.Unavailable, message, new InvalidOperationException(message));
    }
}
