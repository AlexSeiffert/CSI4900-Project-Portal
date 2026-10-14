using System.ComponentModel.DataAnnotations;
using API.DTOs.Mapping;
using API.DTOs.Request;
using API.Exceptions;
using API.Repositories.Interfaces;
using API.Services.Interfaces;

namespace API.Services;

public class StudentService(IStudentRepository repository) : IStudentService
{
    private readonly IStudentRepository _repository = repository;

    public async Task<IEnumerable<DTOs.Student>> GetStudents(CancellationToken cancellationToken = default)
    {
        try
        {
            var profiles = await _repository.GetStudents(cancellationToken);
            return profiles.Select(profile => profile.ToDto()).ToList();
        }
        catch (RepositoryException exception)
        {
            throw new ServiceException(nameof(GetStudents), exception.Type, "The data operation could not be completed.", exception);
        }
        catch (ValidationException exception)
        {
            throw new ServiceException(nameof(GetStudents), ErrorType.Validation, exception.Message, exception);
        }
        catch (OperationCanceledException exception)
        {
            throw new ServiceException(nameof(GetStudents), ErrorType.Cancelled, "The operation was cancelled.", exception);
        }
        catch (Exception exception) when (exception is not ServiceException)
        {
            throw new ServiceException(nameof(GetStudents), ErrorType.Unexpected, "The operation could not be completed.", exception);
        }
    }

    public async Task<DTOs.Student> GetStudentById(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var profile = await _repository.GetStudentById(userId, cancellationToken);
            if (profile == null)
            {
                var exception = new KeyNotFoundException($"Student '{userId}' was not found.");
                throw new ServiceException(nameof(GetStudentById), ErrorType.NotFound, exception.Message, exception);
            }
            return profile.ToDto();
        }
        catch (RepositoryException exception)
        {
            throw new ServiceException(nameof(GetStudentById), exception.Type, "The data operation could not be completed.", exception);
        }
        catch (ValidationException exception)
        {
            throw new ServiceException(nameof(GetStudentById), ErrorType.Validation, exception.Message, exception);
        }
        catch (OperationCanceledException exception)
        {
            throw new ServiceException(nameof(GetStudentById), ErrorType.Cancelled, "The operation was cancelled.", exception);
        }
        catch (Exception exception) when (exception is not ServiceException)
        {
            throw new ServiceException(nameof(GetStudentById), ErrorType.Unexpected, "The operation could not be completed.", exception);
        }
    }

    public async Task<DTOs.Student> UpdateStudent(Guid userId, UpdateStudent request, CancellationToken cancellationToken = default)
    {
        try
        {
            Validator.ValidateObject(request, new ValidationContext(request), true);
            var profile = await _repository.GetStudentById(userId, cancellationToken);
            if (profile == null)
            {
                var exception = new KeyNotFoundException($"Student '{userId}' was not found.");
                throw new ServiceException(nameof(UpdateStudent), ErrorType.NotFound, exception.Message, exception);
            }
            profile.DisplayName = request.DisplayName;
            profile.Program = request.Program;
            profile.UpdatedAt = DateTime.UtcNow;
            return (await _repository.UpdateStudent(profile, request.TopicIds, cancellationToken)).ToDto();
        }
        catch (RepositoryException exception)
        {
            throw new ServiceException(nameof(UpdateStudent), exception.Type, "The data operation could not be completed.", exception);
        }
        catch (ValidationException exception)
        {
            throw new ServiceException(nameof(UpdateStudent), ErrorType.Validation, exception.Message, exception);
        }
        catch (OperationCanceledException exception)
        {
            throw new ServiceException(nameof(UpdateStudent), ErrorType.Cancelled, "The operation was cancelled.", exception);
        }
        catch (Exception exception) when (exception is not ServiceException)
        {
            throw new ServiceException(nameof(UpdateStudent), ErrorType.Unexpected, "The operation could not be completed.", exception);
        }
    }
}
