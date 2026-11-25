using API.DTOs;
using API.DTOs.Request;

namespace API.Services.Interfaces;

public interface IInquiryService
{
    Task<IEnumerable<Inquiry>> GetInquiries(CancellationToken cancellationToken = default);
    Task<IEnumerable<Inquiry>> GetInquiriesByStudentId(Guid studentId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Inquiry>> GetInquiriesBySupervisorId(Guid supervisorId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Inquiry>> GetInquiriesByProjectId(Guid projectId, CancellationToken cancellationToken = default);
    Task<Inquiry> GetInquiryById(Guid inquiryId, Caller caller, CancellationToken cancellationToken = default);
    Task<Inquiry> CreateInquiry(CreateInquiry request, CancellationToken cancellationToken = default);
    Task<Inquiry> AddMember(Guid inquiryId, AddInquiryMember request, Caller caller, CancellationToken cancellationToken = default);
    Task RemoveMember(Guid inquiryId, Guid studentId, Caller caller, CancellationToken cancellationToken = default);
    Task<Inquiry> MarkSent(Guid inquiryId, Caller caller, CancellationToken cancellationToken = default);
    Task<Inquiry> Offer(Guid inquiryId, Caller caller, CancellationToken cancellationToken = default);
    Task<Inquiry> Close(Guid inquiryId, Caller caller, CancellationToken cancellationToken = default);
    Task<RespondResult> Respond(Guid inquiryId, bool accept, Caller caller, CancellationToken cancellationToken = default);
    Task<SignedUrl> CreateUploadUrl(Guid inquiryId, CreateUploadUrl request, Caller caller, CancellationToken cancellationToken = default);
    Task<InquiryAttachment> RegisterAttachment(Guid inquiryId, RegisterAttachment request, Caller caller, CancellationToken cancellationToken = default);
    Task<SignedUrl> CreateDownloadUrl(Guid inquiryId, Guid attachmentId, Caller caller, CancellationToken cancellationToken = default);
}
