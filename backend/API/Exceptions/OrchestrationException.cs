namespace API.Exceptions;

public class OrchestrationException(string operation, ErrorType kind, string message, Exception innerException) : Exception(message, innerException)
{
    public string Operation { get; } = operation;
    public ErrorType Kind { get; } = kind;

    public OrchestrationException(string operation, ServiceException innerException)
        : this(operation, innerException.Kind, innerException.Message, innerException)
    {

    }
}
