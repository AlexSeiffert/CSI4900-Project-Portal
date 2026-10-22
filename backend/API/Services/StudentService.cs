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
        var profiles = await _repository.GetStudents(cancellationToken);
        return profiles.Select(profile => profile.ToDto()).ToList();
    }

    public async Task<DTOs.Student> GetStudentById(Guid userId, CancellationToken cancellationToken = default)
    {
        var profile = await _repository.GetStudentById(userId, cancellationToken)
            ?? throw ServiceException.NotFound(nameof(GetStudentById), $"Student '{userId}' was not found.");
        return profile.ToDtoWithRelations();
    }

    public async Task<DTOs.Student> UpdateStudent(Guid userId, UpdateStudent request, CancellationToken cancellationToken = default)
    {
        var profile = await _repository.GetStudentById(userId, cancellationToken)
            ?? throw ServiceException.NotFound(nameof(UpdateStudent), $"Student '{userId}' was not found.");
        profile.DisplayName = request.DisplayName;
        profile.Program = request.Program;
        return (await _repository.UpdateStudent(profile, request.TopicIds, cancellationToken)).ToDtoWithRelations();
    }
}
