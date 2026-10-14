using API.Models;

namespace API.Repositories.Interfaces;

public interface IInquiryRepository
{
    Task<List<Inquiry>> GetInquiries(CancellationToken cancellationToken = default);
    Task<List<Inquiry>> GetInquiriesByStudentId(Guid studentId, CancellationToken cancellationToken = default);
    Task<List<Inquiry>> GetInquiriesBySupervisorId(Guid supervisorId, CancellationToken cancellationToken = default);
    Task<List<Inquiry>> GetInquiriesByProjectId(Guid projectId, CancellationToken cancellationToken = default);
    Task<Inquiry?> GetInquiryById(Guid inquiryId, CancellationToken cancellationToken = default);
    Task<Inquiry> CreateInquiry(Inquiry inquiry, CancellationToken cancellationToken = default);
    Task<Inquiry> UpdateInquiry(Inquiry inquiry, CancellationToken cancellationToken = default);
}
