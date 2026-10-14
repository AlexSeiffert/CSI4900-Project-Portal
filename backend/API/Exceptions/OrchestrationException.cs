namespace API.Exceptions;

public class OrchestrationException(string operation, ErrorKind kind, string message, Exception innerException) : Exception(message, innerException)
{
    public string Operation { get; } = operation;
    public ErrorKind Kind { get; } = kind;

    public OrchestrationException(string operation, ServiceException innerException)
        : this(operation, innerException.Kind, innerException.Message, innerException)
    {

    }
}
