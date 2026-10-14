using API.Models;

namespace API.Repositories.Interfaces;

public interface IStudentRepository
{
    Task<List<Student>> GetStudents(CancellationToken cancellationToken = default);
    Task<Student?> GetStudentById(Guid userId, CancellationToken cancellationToken = default);
    Task<Student> UpdateStudent(Student profile, IReadOnlyCollection<Guid> topicIds, CancellationToken cancellationToken = default);
}
