using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace API.Exceptions;

public class RepositoryException : Exception
{
    public ErrorKind Kind { get; }

    public RepositoryException(string operation, Exception innerException)
        : base($"Repository operation '{operation}' failed.", innerException)
    {
        Kind = innerException switch
        {
            RepositoryException repository => repository.Kind,
            OperationCanceledException => ErrorKind.Cancelled,
            DbUpdateConcurrencyException => ErrorKind.Conflict,
            DbUpdateException { InnerException: PostgresException { SqlState: PostgresErrorCodes.UniqueViolation or PostgresErrorCodes.ForeignKeyViolation } } => ErrorKind.Conflict,
            _ => ErrorKind.Unexpected
        };
    }
}
