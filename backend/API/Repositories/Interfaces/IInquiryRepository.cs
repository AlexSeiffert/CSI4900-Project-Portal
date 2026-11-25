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
    Task<Guid?> FindStudentIdByEmail(string email, CancellationToken cancellationToken = default);
    Task<Dictionary<Guid, string>> GetMemberEmails(Guid inquiryId, CancellationToken cancellationToken = default);
    Task AddMember(InquiryMember member, CancellationToken cancellationToken = default);
    Task<bool> RemoveMember(Guid inquiryId, Guid studentId, CancellationToken cancellationToken = default);
    Task SaveInquiry(Inquiry inquiry, CancellationToken cancellationToken = default);
    Task<RespondOutcome> Respond(Guid inquiryId, Guid studentId, bool accept, CancellationToken cancellationToken = default);
    Task<InquiryAttachment> AddAttachment(InquiryAttachment attachment, CancellationToken cancellationToken = default);
}
