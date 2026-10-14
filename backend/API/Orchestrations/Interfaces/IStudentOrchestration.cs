using API.DTOs.Request;

namespace API.Orchestrations.Interfaces;

public interface IStudentOrchestration
{
    Task<DTOs.Student> UpdateStudent(Guid userId, UpdateStudent request, CancellationToken cancellationToken = default);
}
