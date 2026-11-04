using API.DTOs.Mapping;
using API.DTOs.Request;
using API.Exceptions;
using API.Models;
using API.Repositories.Interfaces;
using API.Services.Interfaces;

namespace API.Services;

public class InquiryService(IInquiryRepository repository) : IInquiryService
{
    private readonly IInquiryRepository _repository = repository;

    public async Task<IEnumerable<DTOs.Inquiry>> GetInquiries(CancellationToken cancellationToken = default)
    {
        var inquiries = await _repository.GetInquiries(cancellationToken);
        return inquiries.Select(inquiry => inquiry.ToDto()).ToList();
    }

    public async Task<IEnumerable<DTOs.Inquiry>> GetInquiriesByStudentId(Guid studentId, CancellationToken cancellationToken = default)
    {
        var inquiries = await _repository.GetInquiriesByStudentId(studentId, cancellationToken);
        return inquiries.Select(inquiry => inquiry.ToDto()).ToList();
    }

    public async Task<IEnumerable<DTOs.Inquiry>> GetInquiriesBySupervisorId(Guid supervisorId, CancellationToken cancellationToken = default)
    {
        var inquiries = await _repository.GetInquiriesBySupervisorId(supervisorId, cancellationToken);
        return inquiries.Select(inquiry => inquiry.ToDto()).ToList();
    }

    public async Task<IEnumerable<DTOs.Inquiry>> GetInquiriesByProjectId(Guid projectId, CancellationToken cancellationToken = default)
    {
        var inquiries = await _repository.GetInquiriesByProjectId(projectId, cancellationToken);
        return inquiries.Select(inquiry => inquiry.ToDto()).ToList();
    }

    public async Task<DTOs.Inquiry> GetInquiryById(Guid inquiryId, CancellationToken cancellationToken = default)
    {
        var inquiry = await _repository.GetInquiryById(inquiryId, cancellationToken)
            ?? throw ServiceException.NotFound(nameof(GetInquiryById), $"Inquiry '{inquiryId}' was not found.");
        return inquiry.ToDtoWithRelations();
    }

    public async Task<DTOs.Inquiry> CreateInquiry(CreateInquiry request, CancellationToken cancellationToken = default)
    {
        var inquiry = new Inquiry
        {
            Id = Guid.NewGuid(),
            StudentId = request.StudentId,
            SupervisorId = request.SupervisorId,
            ProjectId = request.ProjectId,
            Message = request.Message
        };
        return (await _repository.CreateInquiry(inquiry, cancellationToken)).ToDtoWithRelations();
    }
}
