using API.Exceptions;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace API.Tests.Middleware;

// Tests that database errors are turned into the right error type (and the right HTTP status).
public class RepositoryExceptionTests
{
    private static PostgresException Postgres(string sqlState) => new("failed", "ERROR", "ERROR", sqlState);

    [Theory]
    [InlineData(PostgresErrorCodes.UniqueViolation, ErrorType.Conflict)]
    [InlineData(PostgresErrorCodes.ForeignKeyViolation, ErrorType.Validation)]
    [InlineData(PostgresErrorCodes.InsufficientPrivilege, ErrorType.Forbidden)]
    [InlineData(PostgresErrorCodes.DeadlockDetected, ErrorType.Unexpected)]
    public void MapsPostgresErrors_FromSaveChangesAndRawSql(string sqlState, ErrorType expected)
    {
        var raw = new RepositoryException("RawSql", Postgres(sqlState));
        var saved = new RepositoryException("SaveChanges", new DbUpdateException("failed", Postgres(sqlState)));

        Assert.Equal(expected, raw.Type);
        Assert.Equal(expected, saved.Type);
    }

    [Fact]
    public void ConcurrencyFailure_IsConflictUnlessTypeIsGiven()
    {
        var concurrency = new DbUpdateConcurrencyException("no rows");

        Assert.Equal(ErrorType.Conflict, new RepositoryException("Update", concurrency).Type);
        Assert.Equal(ErrorType.Forbidden, new RepositoryException("Update", concurrency, ErrorType.Forbidden).Type);
    }
}
