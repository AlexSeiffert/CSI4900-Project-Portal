namespace API.Exceptions;

public class OrchestrationException(string operation, ErrorType type, string message, Exception innerException) : Exception(message, innerException)
{
    public string Operation { get; } = operation;
    public ErrorType Type { get; } = type;

    public OrchestrationException(string operation, ServiceException innerException)
        : this(operation, innerException.Type, innerException.Message, innerException)
    {

    }
}
