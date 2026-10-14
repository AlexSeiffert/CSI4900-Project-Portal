using System.ComponentModel.DataAnnotations;
using API.DTOs.Mapping;
using API.DTOs.Request;
using API.Exceptions;
using API.Repositories.Interfaces;
using API.Services.Interfaces;

namespace API.Services;

public class SupervisorService(ISupervisorRepository repository) : ISupervisorService
{
    private readonly ISupervisorRepository _repository = repository;

    public async Task<IEnumerable<DTOs.Supervisor>> GetSupervisors(CancellationToken cancellationToken = default)
    {
        try
        {
            var profiles = await _repository.GetSupervisors(cancellationToken);
            return profiles.Select(profile => profile.ToDto()).ToList();
        }
        catch (RepositoryException exception)
        {
            throw new ServiceException(nameof(GetSupervisors), exception.Type, "The data operation could not be completed.", exception);
        }
        catch (ValidationException exception)
        {
            throw new ServiceException(nameof(GetSupervisors), ErrorType.Validation, exception.Message, exception);
        }
        catch (OperationCanceledException exception)
        {
            throw new ServiceException(nameof(GetSupervisors), ErrorType.Cancelled, "The operation was cancelled.", exception);
        }
        catch (Exception exception) when (exception is not ServiceException)
        {
            throw new ServiceException(nameof(GetSupervisors), ErrorType.Unexpected, "The operation could not be completed.", exception);
        }
    }

    public async Task<DTOs.Supervisor> GetSupervisorById(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var profile = await _repository.GetSupervisorById(userId, cancellationToken);
            if (profile == null)
            {
                var exception = new KeyNotFoundException($"Supervisor '{userId}' was not found.");
                throw new ServiceException(nameof(GetSupervisorById), ErrorType.NotFound, exception.Message, exception);
            }
            return profile.ToDtoWithRelations();
        }
        catch (RepositoryException exception)
        {
            throw new ServiceException(nameof(GetSupervisorById), exception.Type, "The data operation could not be completed.", exception);
        }
        catch (ValidationException exception)
        {
            throw new ServiceException(nameof(GetSupervisorById), ErrorType.Validation, exception.Message, exception);
        }
        catch (OperationCanceledException exception)
        {
            throw new ServiceException(nameof(GetSupervisorById), ErrorType.Cancelled, "The operation was cancelled.", exception);
        }
        catch (Exception exception) when (exception is not ServiceException)
        {
            throw new ServiceException(nameof(GetSupervisorById), ErrorType.Unexpected, "The operation could not be completed.", exception);
        }
    }

    public async Task<DTOs.Supervisor> UpdateSupervisor(Guid userId, UpdateSupervisor request, CancellationToken cancellationToken = default)
    {
        try
        {
            Validator.ValidateObject(request, new ValidationContext(request), true);
            var profile = await _repository.GetSupervisorById(userId, cancellationToken);
            if (profile == null)
            {
                var exception = new KeyNotFoundException($"Supervisor '{userId}' was not found.");
                throw new ServiceException(nameof(UpdateSupervisor), ErrorType.NotFound, exception.Message, exception);
            }
            profile.DisplayName = request.DisplayName;
            profile.ContactEmail = request.ContactEmail;
            profile.AboutMe = request.AboutMe;
            profile.ExternalUrl = request.ExternalUrl;
            profile.AcceptingInquiries = request.AcceptingInquiries;
            profile.UpdatedAt = DateTime.UtcNow;
            return (await _repository.UpdateSupervisor(profile, request.TopicIds, cancellationToken)).ToDtoWithRelations();
        }
        catch (RepositoryException exception)
        {
            throw new ServiceException(nameof(UpdateSupervisor), exception.Type, "The data operation could not be completed.", exception);
        }
        catch (ValidationException exception)
        {
            throw new ServiceException(nameof(UpdateSupervisor), ErrorType.Validation, exception.Message, exception);
        }
        catch (OperationCanceledException exception)
        {
            throw new ServiceException(nameof(UpdateSupervisor), ErrorType.Cancelled, "The operation was cancelled.", exception);
        }
        catch (Exception exception) when (exception is not ServiceException)
        {
            throw new ServiceException(nameof(UpdateSupervisor), ErrorType.Unexpected, "The operation could not be completed.", exception);
        }
    }
}
