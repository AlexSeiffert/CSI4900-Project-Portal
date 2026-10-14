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
            throw new ServiceException(nameof(GetInquiries), exception.Kind, "The data operation could not be completed.", exception);
        }
        catch (ValidationException exception)
        {
            throw new ServiceException(nameof(GetInquiries), ErrorType.Validation, exception.Message, exception);
        }
        catch (OperationCanceledException exception)
        {
            throw new ServiceException(nameof(GetInquiries), ErrorType.Cancelled, "The operation was cancelled.", exception);
        }
        catch (Exception exception) when (exception is not ServiceException)
        {
            throw new ServiceException(nameof(GetInquiries), ErrorType.Unexpected, "The operation could not be completed.", exception);
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
            throw new ServiceException(nameof(GetInquiriesByStudentId), exception.Kind, "The data operation could not be completed.", exception);
        }
        catch (ValidationException exception)
        {
            throw new ServiceException(nameof(GetInquiriesByStudentId), ErrorType.Validation, exception.Message, exception);
        }
        catch (OperationCanceledException exception)
        {
            throw new ServiceException(nameof(GetInquiriesByStudentId), ErrorType.Cancelled, "The operation was cancelled.", exception);
        }
        catch (Exception exception) when (exception is not ServiceException)
        {
            throw new ServiceException(nameof(GetInquiriesByStudentId), ErrorType.Unexpected, "The operation could not be completed.", exception);
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
            throw new ServiceException(nameof(GetInquiriesBySupervisorId), exception.Kind, "The data operation could not be completed.", exception);
        }
        catch (ValidationException exception)
        {
            throw new ServiceException(nameof(GetInquiriesBySupervisorId), ErrorType.Validation, exception.Message, exception);
        }
        catch (OperationCanceledException exception)
        {
            throw new ServiceException(nameof(GetInquiriesBySupervisorId), ErrorType.Cancelled, "The operation was cancelled.", exception);
        }
        catch (Exception exception) when (exception is not ServiceException)
        {
            throw new ServiceException(nameof(GetInquiriesBySupervisorId), ErrorType.Unexpected, "The operation could not be completed.", exception);
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
            throw new ServiceException(nameof(GetInquiriesByProjectId), exception.Kind, "The data operation could not be completed.", exception);
        }
        catch (ValidationException exception)
        {
            throw new ServiceException(nameof(GetInquiriesByProjectId), ErrorType.Validation, exception.Message, exception);
        }
        catch (OperationCanceledException exception)
        {
            throw new ServiceException(nameof(GetInquiriesByProjectId), ErrorType.Cancelled, "The operation was cancelled.", exception);
        }
        catch (Exception exception) when (exception is not ServiceException)
        {
            throw new ServiceException(nameof(GetInquiriesByProjectId), ErrorType.Unexpected, "The operation could not be completed.", exception);
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
            return inquiry.ToDto();
        }
        catch (RepositoryException exception)
        {
            throw new ServiceException(nameof(GetInquiryById), exception.Kind, "The data operation could not be completed.", exception);
        }
        catch (ValidationException exception)
        {
            throw new ServiceException(nameof(GetInquiryById), ErrorType.Validation, exception.Message, exception);
        }
        catch (OperationCanceledException exception)
        {
            throw new ServiceException(nameof(GetInquiryById), ErrorType.Cancelled, "The operation was cancelled.", exception);
        }
        catch (Exception exception) when (exception is not ServiceException)
        {
            throw new ServiceException(nameof(GetInquiryById), ErrorType.Unexpected, "The operation could not be completed.", exception);
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
            return (await _repository.CreateInquiry(inquiry, cancellationToken)).ToDto();
        }
        catch (RepositoryException exception)
        {
            throw new ServiceException(nameof(CreateInquiry), exception.Kind, "The data operation could not be completed.", exception);
        }
        catch (ValidationException exception)
        {
            throw new ServiceException(nameof(CreateInquiry), ErrorType.Validation, exception.Message, exception);
        }
        catch (OperationCanceledException exception)
        {
            throw new ServiceException(nameof(CreateInquiry), ErrorType.Cancelled, "The operation was cancelled.", exception);
        }
        catch (Exception exception) when (exception is not ServiceException)
        {
            throw new ServiceException(nameof(CreateInquiry), ErrorType.Unexpected, "The operation could not be completed.", exception);
        }
    }

    public async Task<DTOs.Inquiry> UpdateInquiry(Guid inquiryId, UpdateInquiry request, CancellationToken cancellationToken = default)
    {
        try
        {
            Validator.ValidateObject(request, new ValidationContext(request), true);
            var inquiry = await _repository.GetInquiryById(inquiryId, cancellationToken);
            if (inquiry == null)
            {
                var exception = new KeyNotFoundException($"Inquiry '{inquiryId}' was not found.");
                throw new ServiceException(nameof(UpdateInquiry), ErrorType.NotFound, exception.Message, exception);
            }
            inquiry.Status = request.Status!.Value;
            return (await _repository.UpdateInquiry(inquiry, cancellationToken)).ToDto();
        }
        catch (RepositoryException exception)
        {
            throw new ServiceException(nameof(UpdateInquiry), exception.Kind, "The data operation could not be completed.", exception);
        }
        catch (ValidationException exception)
        {
            throw new ServiceException(nameof(UpdateInquiry), ErrorType.Validation, exception.Message, exception);
        }
        catch (OperationCanceledException exception)
        {
            throw new ServiceException(nameof(UpdateInquiry), ErrorType.Cancelled, "The operation was cancelled.", exception);
        }
        catch (Exception exception) when (exception is not ServiceException)
        {
            throw new ServiceException(nameof(UpdateInquiry), ErrorType.Unexpected, "The operation could not be completed.", exception);
        }
    }
}
