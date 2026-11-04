using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace API.Exceptions;

// Wraps any error from the database layer and decides what kind of error it is (the Type),
// which later becomes the HTTP status code (see ApiExceptionHandler).
public class RepositoryException : Exception
{
    public ErrorType Type { get; }

    // Use this when the caller already knows the error type.
    public RepositoryException(string operation, Exception innerException, ErrorType type)
        : base($"Repository operation '{operation}' failed.", innerException)
    {
        Type = type;
    }

    // Works out the error type from the original exception.
    public RepositoryException(string operation, Exception innerException)
        : base($"Repository operation '{operation}' failed.", innerException)
    {
        Type = innerException switch
        {
            RepositoryException repository => repository.Type,
            OperationCanceledException => ErrorType.Cancelled,
            DbUpdateConcurrencyException => ErrorType.Conflict,
            // Unique and foreign key errors can arrive two ways: wrapped in a DbUpdateException (normal saves)
            // or as a plain PostgresException (raw SQL queries). Both are handled.
            // Duplicate value: 409.
            PostgresException { SqlState: PostgresErrorCodes.UniqueViolation }
                or DbUpdateException { InnerException: PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } } => ErrorType.Conflict,
            // Points at something that does not exist (for example an unknown supervisor id): 400.
            PostgresException { SqlState: PostgresErrorCodes.ForeignKeyViolation }
                or DbUpdateException { InnerException: PostgresException { SqlState: PostgresErrorCodes.ForeignKeyViolation } } => ErrorType.Validation,
            // A permission rule in the database (row level security) blocked the action.
            PostgresException { SqlState: PostgresErrorCodes.InsufficientPrivilege }
                or DbUpdateException { InnerException: PostgresException { SqlState: PostgresErrorCodes.InsufficientPrivilege } } => ErrorType.Forbidden,
            _ => ErrorType.Unexpected
        };
    }
}
