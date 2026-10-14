using API.DTOs.Request;

namespace API.Orchestrations.Interfaces;

public interface IInquiryOrchestration
{
    Task<DTOs.Inquiry> CreateInquiry(CreateInquiry request, CancellationToken cancellationToken = default);
}
