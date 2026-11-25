using API.Exceptions;
using API.Models;
using API.Models.Data;
using API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace API.Repositories;

public class InquiryRepository(ApplicationDbContext db) : IInquiryRepository
{
    private readonly ApplicationDbContext _db = db;

    public async Task<List<Inquiry>> GetInquiries(CancellationToken cancellationToken = default)
    {
        try
        {
            return await InquiryQuery().ToListAsync(cancellationToken);
        }
        catch (Exception exception) when (exception is not RepositoryException)
        {
            throw new RepositoryException(nameof(GetInquiries), exception);
        }
    }

    // The inquiries the student is a member of (a creator who declined has left).
    public async Task<List<Inquiry>> GetInquiriesByStudentId(Guid studentId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await InquiryQuery().Where(inquiry => inquiry.Members.Any(member => member.StudentId == studentId))
                .ToListAsync(cancellationToken);
        }
        catch (Exception exception) when (exception is not RepositoryException)
        {
            throw new RepositoryException(nameof(GetInquiriesByStudentId), exception);
        }
    }

    public async Task<List<Inquiry>> GetInquiriesBySupervisorId(Guid supervisorId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await InquiryQuery().Where(inquiry => inquiry.SupervisorId == supervisorId).ToListAsync(cancellationToken);
        }
        catch (Exception exception) when (exception is not RepositoryException)
        {
            throw new RepositoryException(nameof(GetInquiriesBySupervisorId), exception);
        }
    }

    public async Task<List<Inquiry>> GetInquiriesByProjectId(Guid projectId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await InquiryQuery().Where(inquiry => inquiry.ProjectId == projectId).ToListAsync(cancellationToken);
        }
        catch (Exception exception) when (exception is not RepositoryException)
        {
            throw new RepositoryException(nameof(GetInquiriesByProjectId), exception);
        }
    }

    public async Task<Inquiry?> GetInquiryById(Guid inquiryId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await InquiryWithRelations().SingleOrDefaultAsync(inquiry => inquiry.Id == inquiryId, cancellationToken);
        }
        catch (Exception exception) when (exception is not RepositoryException)
        {
            throw new RepositoryException(nameof(GetInquiryById), exception);
        }
    }

    public async Task<Inquiry> CreateInquiry(Inquiry inquiry, CancellationToken cancellationToken = default)
    {
        try
        {
            _db.Inquiries.Add(inquiry);
            await _db.SaveChangesAsync(cancellationToken);
            return await InquiryWithRelations().SingleAsync(saved => saved.Id == inquiry.Id, cancellationToken);
        }
        catch (Exception exception) when (exception is not RepositoryException)
        {
            throw new RepositoryException(nameof(CreateInquiry), exception);
        }
    }

    // Null when no student has that email. Postgres never lets the caller read other users' rows,
    // so it asks find_student_by_email() (migration 0009) instead.
    public async Task<Guid?> FindStudentIdByEmail(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            var normalized = email.Trim().ToLowerInvariant();
            if (!_db.Database.IsRelational())
            {
                return await _db.Users.Where(user => user.Email.ToLower() == normalized
                        && _db.Students.Any(student => student.UserId == user.Id))
                    .Select(user => (Guid?)user.Id).FirstOrDefaultAsync(cancellationToken);
            }
            try
            {
                return await _db.Database.SqlQuery<Guid>($"select public.find_student_by_email({normalized}) as \"Value\"")
                    .SingleAsync(cancellationToken);
            }
            catch (Exception exception) when (IsNotFound(exception))
            {
                return null;
            }
        }
        catch (Exception exception) when (exception is not RepositoryException)
        {
            throw new RepositoryException(nameof(FindStudentIdByEmail), exception);
        }
    }

    public async Task<Dictionary<Guid, string>> GetMemberEmails(Guid inquiryId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_db.Database.IsRelational())
            {
                return await _db.InquiryMembers.Where(member => member.InquiryId == inquiryId)
                    .Join(_db.Users, member => member.StudentId, user => user.Id, (member, user) => new { member.StudentId, user.Email })
                    .ToDictionaryAsync(row => row.StudentId, row => row.Email, cancellationToken);
            }
            var rows = await _db.Database.SqlQuery<MemberEmailRow>(
                    $"select student_id as \"StudentId\", email as \"Email\" from public.inquiry_member_emails({inquiryId})")
                .ToListAsync(cancellationToken);
            return rows.ToDictionary(row => row.StudentId, row => row.Email);
        }
        catch (Exception exception) when (exception is not RepositoryException)
        {
            throw new RepositoryException(nameof(GetMemberEmails), exception);
        }
    }

    public async Task AddMember(InquiryMember member, CancellationToken cancellationToken = default)
    {
        try
        {
            _db.InquiryMembers.Add(member);
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (Exception exception) when (exception is not RepositoryException)
        {
            throw new RepositoryException(nameof(AddMember), exception);
        }
    }

    public async Task<bool> RemoveMember(Guid inquiryId, Guid studentId, CancellationToken cancellationToken = default)
    {
        try
        {
            var member = await _db.InquiryMembers
                .SingleOrDefaultAsync(row => row.InquiryId == inquiryId && row.StudentId == studentId, cancellationToken);
            if (member == null)
            {
                return false;
            }
            await DeleteMember(member, cancellationToken);
            return true;
        }
        catch (Exception exception) when (exception is not RepositoryException)
        {
            throw new RepositoryException(nameof(RemoveMember), exception);
        }
    }

    public async Task SaveInquiry(Inquiry inquiry, CancellationToken cancellationToken = default)
    {
        try
        {
            // Returns Forbidden if the database blocks this user from changing the inquiry.
            await _db.SaveChangesOrForbiddenAsync(nameof(SaveInquiry), cancellationToken);
        }
        catch (Exception exception) when (exception is not RepositoryException)
        {
            throw new RepositoryException(nameof(SaveInquiry), exception);
        }
    }

    // Records the student's answer to an offer and, once nobody is pending, settles the inquiry.
    // Accepting also declines every other offer the student is still pending on. All of it is one
    // transaction. Each step is saved on its own because the database checks the members' answers
    // before letting the status move.
    public async Task<RespondOutcome> Respond(Guid inquiryId, Guid studentId, bool accept, CancellationToken cancellationToken = default)
    {
        try
        {
            _db.ChangeTracker.Clear();
            var relational = _db.Database.IsRelational();
            await using var transaction = relational ? await _db.Database.BeginTransactionAsync(cancellationToken) : null;
            var status = await Answer(inquiryId, studentId, accept, relational, cancellationToken);
            var dropped = new List<Guid>();
            if (accept)
            {
                var others = await _db.InquiryMembers
                    .Where(member => member.StudentId == studentId && member.InquiryId != inquiryId
                        && member.Response == InquiryResponse.Pending && member.Inquiry.Status == InquiryStatus.Offered)
                    .Select(member => member.InquiryId).ToListAsync(cancellationToken);
                foreach (var other in others)
                {
                    await Answer(other, studentId, false, relational, cancellationToken);
                    dropped.Add(other);
                }
            }
            if (transaction != null)
            {
                await transaction.CommitAsync(cancellationToken);
            }
            return new RespondOutcome(status, dropped);
        }
        catch (Exception exception) when (exception is not RepositoryException)
        {
            throw new RepositoryException(nameof(Respond), exception);
        }
    }

    public async Task<InquiryAttachment> AddAttachment(InquiryAttachment attachment, CancellationToken cancellationToken = default)
    {
        try
        {
            _db.InquiryAttachments.Add(attachment);
            await _db.SaveChangesAsync(cancellationToken);
            return attachment;
        }
        catch (Exception exception) when (exception is not RepositoryException)
        {
            throw new RepositoryException(nameof(AddAttachment), exception);
        }
    }

    private async Task<InquiryStatus> Answer(Guid inquiryId, Guid studentId, bool accept, bool relational, CancellationToken cancellationToken)
    {
        if (relational)
        {
            // Two members answering at once must not both see the other as still pending.
            await _db.Database.ExecuteSqlAsync($"select 1 from public.inquiries where id = {inquiryId} for update", cancellationToken);
        }
        var inquiry = await _db.Inquiries.Include(row => row.Members).SingleAsync(row => row.Id == inquiryId, cancellationToken);
        var member = inquiry.Members.SingleOrDefault(row => row.StudentId == studentId);
        if (inquiry.Status != InquiryStatus.Offered || member is not { Response: InquiryResponse.Pending })
        {
            throw new RepositoryException(nameof(Respond),
                new InvalidOperationException("The offer is no longer open for this student."), ErrorType.Conflict);
        }

        member.Response = accept ? InquiryResponse.Accepted : InquiryResponse.Declined;
        await _db.SaveChangesOrForbiddenAsync(nameof(Respond), cancellationToken);
        if (InquiryRules.Outcome(inquiry.Members) is { } outcome)
        {
            inquiry.Status = outcome;
            await _db.SaveChangesOrForbiddenAsync(nameof(Respond), cancellationToken);
        }
        if (!accept)
        {
            await DeleteMember(member, cancellationToken);
        }
        return inquiry.Status;
    }

    private async Task DeleteMember(InquiryMember member, CancellationToken cancellationToken)
    {
        if (!_db.Database.IsRelational())
        {
            // Postgres does this itself (remove_member_attachments() in migration 0010).
            var attachments = await _db.InquiryAttachments
                .Where(row => row.InquiryId == member.InquiryId && row.StudentId == member.StudentId).ToListAsync(cancellationToken);
            _db.InquiryAttachments.RemoveRange(attachments);
        }
        _db.InquiryMembers.Remove(member);
        // Returns Forbidden if the database blocks this user from removing the member.
        await _db.SaveChangesOrForbiddenAsync(nameof(RemoveMember), cancellationToken);
    }

    private static bool IsNotFound(Exception exception)
    {
        return exception is PostgresException { SqlState: PostgresErrorCodes.NoDataFound }
            || exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.NoDataFound };
    }

    private IQueryable<Inquiry> InquiryQuery()
    {
        return _db.Inquiries.AsNoTracking().Include(inquiry => inquiry.Members)
            .OrderByDescending(inquiry => inquiry.CreatedAt).ThenBy(inquiry => inquiry.Id);
    }

    private IQueryable<Inquiry> InquiryWithRelations()
    {
        return _db.Inquiries
            .Include(inquiry => inquiry.Student)
            .Include(inquiry => inquiry.Supervisor)
            .Include(inquiry => inquiry.Project)
            .Include(inquiry => inquiry.Members).ThenInclude(member => member.Student)
            .Include(inquiry => inquiry.Attachments)
            .AsSplitQuery();
    }

    private sealed class MemberEmailRow
    {
        public Guid StudentId { get; set; }
        public string Email { get; set; } = string.Empty;
    }
}
