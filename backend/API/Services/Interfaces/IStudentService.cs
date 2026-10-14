using API.DTOs.Request;

namespace API.Services.Interfaces;

public interface IStudentService
{
    Task<IEnumerable<DTOs.Student>> GetStudents(CancellationToken cancellationToken = default);
    Task<DTOs.Student> GetStudentById(Guid userId, CancellationToken cancellationToken = default);
    Task<DTOs.Student> UpdateStudent(Guid userId, UpdateStudent request, CancellationToken cancellationToken = default);
    Task ValidateStudentExists(Guid studentId, CancellationToken cancellationToken = default);
}
