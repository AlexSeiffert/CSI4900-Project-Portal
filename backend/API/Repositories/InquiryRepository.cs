using API.Exceptions;
using API.Models;
using API.Models.Data;
using API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

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

    public async Task<List<Inquiry>> GetInquiriesByStudentId(Guid studentId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await InquiryQuery().Where(inquiry => inquiry.StudentId == studentId).ToListAsync(cancellationToken);
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
            return await _db.Inquiries.FindAsync([inquiryId], cancellationToken);
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
            return inquiry;
        }
        catch (Exception exception) when (exception is not RepositoryException)
        {
            throw new RepositoryException(nameof(CreateInquiry), exception);
        }
    }


    private IQueryable<Inquiry> InquiryQuery()
    {
        return _db.Inquiries.AsNoTracking().OrderByDescending(inquiry => inquiry.CreatedAt).ThenBy(inquiry => inquiry.Id);
    }
}
