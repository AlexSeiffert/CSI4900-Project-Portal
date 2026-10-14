using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace API.Exceptions;

public class RepositoryException : Exception
{
    public ErrorType Type { get; }

    public RepositoryException(string operation, Exception innerException)
        : base($"Repository operation '{operation}' failed.", innerException)
    {
        Type = innerException switch
        {
            RepositoryException repository => repository.Type,
            OperationCanceledException => ErrorType.Cancelled,
            DbUpdateConcurrencyException => ErrorType.Conflict,
            DbUpdateException { InnerException: PostgresException { SqlState: PostgresErrorCodes.UniqueViolation or PostgresErrorCodes.ForeignKeyViolation } } => ErrorType.Conflict,
            _ => ErrorType.Unexpected
        };
    }
}
