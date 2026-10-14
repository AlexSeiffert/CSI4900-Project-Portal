namespace API.Exceptions;

public class ServiceException(string operation, ErrorKind kind, string message, Exception innerException) : Exception(message, innerException)
{
    public string Operation { get; } = operation;
    public ErrorKind Kind { get; } = kind;
}
