using API.DTOs.Request;

namespace API.Services.Interfaces;

public interface IInquiryService
{
    Task<IEnumerable<DTOs.Inquiry>> GetInquiries(CancellationToken cancellationToken = default);
    Task<IEnumerable<DTOs.Inquiry>> GetInquiriesByStudentId(Guid studentId, CancellationToken cancellationToken = default);
    Task<IEnumerable<DTOs.Inquiry>> GetInquiriesBySupervisorId(Guid supervisorId, CancellationToken cancellationToken = default);
    Task<IEnumerable<DTOs.Inquiry>> GetInquiriesByProjectId(Guid projectId, CancellationToken cancellationToken = default);
    Task<DTOs.Inquiry> GetInquiryById(Guid inquiryId, CancellationToken cancellationToken = default);
    Task<DTOs.Inquiry> CreateInquiry(CreateInquiry request, CancellationToken cancellationToken = default);
}
