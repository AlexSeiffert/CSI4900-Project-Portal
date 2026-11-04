using API.Exceptions;
using API.Models.Data;
using Microsoft.EntityFrameworkCore;

namespace API.Repositories;

public static class SaveChangesExtensions
{
    // Use this when updating or deleting a row that already exists.
    // If the database's permission rules (row level security) don't let the user change that row,
    // the database doesn't raise an error. It just changes zero rows, and Entity Framework reports
    // that as a "concurrency" error. This turns that error into Forbidden (403) instead of Conflict (409).
    public static async Task SaveChangesOrForbiddenAsync(this ApplicationDbContext db, string operation, CancellationToken cancellationToken)
    {
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            throw new RepositoryException(operation, exception, ErrorType.Forbidden);
        }
    }
}
