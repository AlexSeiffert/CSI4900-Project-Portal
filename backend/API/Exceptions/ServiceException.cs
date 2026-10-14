namespace API.Exceptions;

public class ServiceException(string operation, ErrorType kind, string message, Exception innerException) : Exception(message, innerException)
{
    public string Operation { get; } = operation;
    public ErrorType Kind { get; } = kind;
}
