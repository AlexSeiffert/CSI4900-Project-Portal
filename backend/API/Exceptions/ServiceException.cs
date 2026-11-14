namespace API.Exceptions;

public class ServiceException(string operation, ErrorType type, string message, Exception innerException) : Exception(message, innerException)
{
    public string Operation { get; } = operation;
    public ErrorType Type { get; } = type;

    public static ServiceException NotFound(string operation, string message)
    {
        return new(operation, ErrorType.NotFound, message, new KeyNotFoundException(message));
    }
}
