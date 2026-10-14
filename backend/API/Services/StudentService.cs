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
            throw new ServiceException(nameof(GetStudents), exception.Kind, "The data operation could not be completed.", exception);
        }
        catch (ValidationException exception)
        {
            throw new ServiceException(nameof(GetStudents), ErrorKind.Validation, exception.Message, exception);
        }
        catch (OperationCanceledException exception)
        {
            throw new ServiceException(nameof(GetStudents), ErrorKind.Cancelled, "The operation was cancelled.", exception);
        }
        catch (Exception exception) when (exception is not ServiceException)
        {
            throw new ServiceException(nameof(GetStudents), ErrorKind.Unexpected, "The operation could not be completed.", exception);
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
                throw new ServiceException(nameof(GetStudentById), ErrorKind.NotFound, exception.Message, exception);
            }
            return profile.ToDto();
        }
        catch (RepositoryException exception)
        {
            throw new ServiceException(nameof(GetStudentById), exception.Kind, "The data operation could not be completed.", exception);
        }
        catch (ValidationException exception)
        {
            throw new ServiceException(nameof(GetStudentById), ErrorKind.Validation, exception.Message, exception);
        }
        catch (OperationCanceledException exception)
        {
            throw new ServiceException(nameof(GetStudentById), ErrorKind.Cancelled, "The operation was cancelled.", exception);
        }
        catch (Exception exception) when (exception is not ServiceException)
        {
            throw new ServiceException(nameof(GetStudentById), ErrorKind.Unexpected, "The operation could not be completed.", exception);
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
                throw new ServiceException(nameof(UpdateStudent), ErrorKind.NotFound, exception.Message, exception);
            }
            profile.DisplayName = request.DisplayName;
            profile.Program = request.Program;
            profile.UpdatedAt = DateTime.UtcNow;
            return (await _repository.UpdateStudent(profile, request.TopicIds, cancellationToken)).ToDto();
        }
        catch (RepositoryException exception)
        {
            throw new ServiceException(nameof(UpdateStudent), exception.Kind, "The data operation could not be completed.", exception);
        }
        catch (ValidationException exception)
        {
            throw new ServiceException(nameof(UpdateStudent), ErrorKind.Validation, exception.Message, exception);
        }
        catch (OperationCanceledException exception)
        {
            throw new ServiceException(nameof(UpdateStudent), ErrorKind.Cancelled, "The operation was cancelled.", exception);
        }
        catch (Exception exception) when (exception is not ServiceException)
        {
            throw new ServiceException(nameof(UpdateStudent), ErrorKind.Unexpected, "The operation could not be completed.", exception);
        }
    }

    public async Task ValidateStudentExists(Guid studentId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (await _repository.GetStudentById(studentId, cancellationToken) == null)
            {
                var exception = new ValidationException("Student does not exist.");
                throw new ServiceException(nameof(ValidateStudentExists), ErrorKind.Validation, exception.Message, exception);
            }
        }
        catch (RepositoryException exception)
        {
            throw new ServiceException(nameof(ValidateStudentExists), exception.Kind, "The data operation could not be completed.", exception);
        }
        catch (ValidationException exception)
        {
            throw new ServiceException(nameof(ValidateStudentExists), ErrorKind.Validation, exception.Message, exception);
        }
        catch (OperationCanceledException exception)
        {
            throw new ServiceException(nameof(ValidateStudentExists), ErrorKind.Cancelled, "The operation was cancelled.", exception);
        }
        catch (Exception exception) when (exception is not ServiceException)
        {
            throw new ServiceException(nameof(ValidateStudentExists), ErrorKind.Unexpected, "The operation could not be completed.", exception);
        }
    }
}
