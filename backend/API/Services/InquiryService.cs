using System.ComponentModel.DataAnnotations;
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
        try
        {
            var inquiries = await _repository.GetInquiries(cancellationToken);
            return inquiries.Select(inquiry => inquiry.ToDto()).ToList();
        }
        catch (RepositoryException exception)
        {
            throw new ServiceException(nameof(GetInquiries), exception.Type, "The data operation could not be completed.", exception);
        }
    }

    public async Task<IEnumerable<DTOs.Inquiry>> GetInquiriesByStudentId(Guid studentId, CancellationToken cancellationToken = default)
    {
        try
        {
            var inquiries = await _repository.GetInquiriesByStudentId(studentId, cancellationToken);
            return inquiries.Select(inquiry => inquiry.ToDto()).ToList();
        }
        catch (RepositoryException exception)
        {
            throw new ServiceException(nameof(GetInquiriesByStudentId), exception.Type, "The data operation could not be completed.", exception);
        }
    }

    public async Task<IEnumerable<DTOs.Inquiry>> GetInquiriesBySupervisorId(Guid supervisorId, CancellationToken cancellationToken = default)
    {
        try
        {
            var inquiries = await _repository.GetInquiriesBySupervisorId(supervisorId, cancellationToken);
            return inquiries.Select(inquiry => inquiry.ToDto()).ToList();
        }
        catch (RepositoryException exception)
        {
            throw new ServiceException(nameof(GetInquiriesBySupervisorId), exception.Type, "The data operation could not be completed.", exception);
        }
    }

    public async Task<IEnumerable<DTOs.Inquiry>> GetInquiriesByProjectId(Guid projectId, CancellationToken cancellationToken = default)
    {
        try
        {
            var inquiries = await _repository.GetInquiriesByProjectId(projectId, cancellationToken);
            return inquiries.Select(inquiry => inquiry.ToDto()).ToList();
        }
        catch (RepositoryException exception)
        {
            throw new ServiceException(nameof(GetInquiriesByProjectId), exception.Type, "The data operation could not be completed.", exception);
        }
    }

    public async Task<DTOs.Inquiry> GetInquiryById(Guid inquiryId, CancellationToken cancellationToken = default)
    {
        try
        {
            var inquiry = await _repository.GetInquiryById(inquiryId, cancellationToken);
            if (inquiry == null)
            {
                var exception = new KeyNotFoundException($"Inquiry '{inquiryId}' was not found.");
                throw new ServiceException(nameof(GetInquiryById), ErrorType.NotFound, exception.Message, exception);
            }
            return inquiry.ToDtoWithRelations();
        }
        catch (RepositoryException exception)
        {
            throw new ServiceException(nameof(GetInquiryById), exception.Type, "The data operation could not be completed.", exception);
        }
    }

    public async Task<DTOs.Inquiry> CreateInquiry(CreateInquiry request, CancellationToken cancellationToken = default)
    {
        try
        {
            Validator.ValidateObject(request, new ValidationContext(request), true);
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
        catch (RepositoryException exception)
        {
            throw new ServiceException(nameof(CreateInquiry), exception.Type, "The data operation could not be completed.", exception);
        }
    }
}
