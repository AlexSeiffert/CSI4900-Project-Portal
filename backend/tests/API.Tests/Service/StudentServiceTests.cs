using API.DTOs.Request;
using API.Exceptions;
using API.Models;
using API.Repositories.Interfaces;
using API.Services;
using Moq;

namespace API.Tests.Service;

public class StudentServiceTests
{
    private readonly Mock<IStudentRepository> _repository = new();

    private StudentService CreateService()
    {
        return new(_repository.Object);
    }

    [Fact]
    public async Task GetStudents_MapsProfilesAndTopics()
    {
        var topic = new Topic { Id = Guid.NewGuid(), Name = "Databases" };
        var student = new Student { UserId = Guid.NewGuid(), DisplayName = "Student", Program = "CS", TopicInterests = [topic] };

        _repository.Setup(repository => repository.GetStudents(CancellationToken.None))
            .ReturnsAsync(new List<Student> { student });

        var service = CreateService();

        var result = Assert.Single(await service.GetStudents(CancellationToken.None));

        Assert.Equal(student.UserId, result.UserId);
        Assert.Equal(student.DisplayName, result.DisplayName);
        Assert.Null(result.TopicInterests);
        Assert.Null(result.Inquiries);
        Assert.Null(result.SavedProjects);
        _repository.Verify(repository => repository.GetStudents(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task GetStudents_PropagatesRepositoryFailure()
    {
        using var cancellation = new CancellationTokenSource();
        var token = cancellation.Token;
        var original = new RepositoryException("dependency", new InvalidOperationException("Database failed"));

        _repository.Setup(repository => repository.GetStudents(token))
            .ThrowsAsync(original);

        var service = CreateService();

        var exception = await Assert.ThrowsAsync<RepositoryException>(() => service.GetStudents(token));

        Assert.Same(original, exception);
        _repository.Verify(repository => repository.GetStudents(token), Times.Once);
    }

    [Fact]
    public async Task GetStudentById_ReturnsMatchingProfile()
    {
        var topic = new Topic { Id = Guid.NewGuid(), Name = "Databases" };
        var student = new Student { UserId = Guid.NewGuid(), DisplayName = "Student", Program = "CS", TopicInterests = [topic] };

        _repository.Setup(repository => repository.GetStudentById(student.UserId, CancellationToken.None))
            .ReturnsAsync(student);

        var service = CreateService();

        var result = await service.GetStudentById(student.UserId, CancellationToken.None);

        Assert.Equal(student.UserId, result.UserId);
        Assert.Equal(student.DisplayName, result.DisplayName);
        Assert.Equal(topic.Id, Assert.Single(result.TopicInterests!).Id);
        _repository.Verify(repository => repository.GetStudentById(student.UserId, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task GetStudentById_PropagatesRepositoryFailure()
    {
        var studentId = Guid.NewGuid();
        using var cancellation = new CancellationTokenSource();
        var token = cancellation.Token;
        var original = new RepositoryException("dependency", new InvalidOperationException("Database failed"));

        _repository.Setup(repository => repository.GetStudentById(studentId, token))
            .ThrowsAsync(original);

        var service = CreateService();

        var exception = await Assert.ThrowsAsync<RepositoryException>(() => service.GetStudentById(studentId, token));

        Assert.Same(original, exception);
        _repository.Verify(repository => repository.GetStudentById(studentId, token), Times.Once);
    }

    [Fact]
    public async Task UpdateStudent_UpdatesFieldsAndPreservesIdentity()
    {
        var topic = new Topic { Id = Guid.NewGuid(), Name = "Databases" };
        var student = new Student { UserId = Guid.NewGuid(), DisplayName = "Student", Program = "CS", TopicInterests = [topic] };
        var request = new UpdateStudent { DisplayName = "Updated", Program = "Engineering", TopicIds = [topic.Id] };
        var before = student.UpdatedAt;

        _repository.Setup(repository => repository.GetStudentById(student.UserId, CancellationToken.None))
            .ReturnsAsync(student);
        _repository.Setup(repository => repository.UpdateStudent(student, request.TopicIds, CancellationToken.None))
            .ReturnsAsync(student);

        var service = CreateService();

        var result = await service.UpdateStudent(student.UserId, request, CancellationToken.None);

        Assert.Equal(student.UserId, result.UserId);
        Assert.Equal(request.DisplayName, result.DisplayName);
        Assert.Equal(request.Program, result.Program);
        Assert.True(result.UpdatedAt >= before);
        _repository.Verify(repository => repository.GetStudentById(student.UserId, CancellationToken.None), Times.Once);
        _repository.Verify(repository => repository.UpdateStudent(student, request.TopicIds, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task UpdateStudent_PropagatesRepositoryFailure()
    {
        var topic = new Topic { Id = Guid.NewGuid(), Name = "Databases" };
        var student = new Student { UserId = Guid.NewGuid(), DisplayName = "Student", Program = "CS", TopicInterests = [topic] };
        var request = new UpdateStudent { DisplayName = "Updated", Program = "Engineering", TopicIds = [topic.Id] };
        using var cancellation = new CancellationTokenSource();
        var token = cancellation.Token;
        var original = new RepositoryException("dependency", new InvalidOperationException("Database failed"));

        _repository.Setup(repository => repository.GetStudentById(student.UserId, token))
            .ReturnsAsync(student);
        _repository.Setup(repository => repository.UpdateStudent(student, request.TopicIds, token))
            .ThrowsAsync(original);

        var service = CreateService();

        var exception = await Assert.ThrowsAsync<RepositoryException>(() => service.UpdateStudent(student.UserId, request, token));

        Assert.Same(original, exception);
        _repository.Verify(repository => repository.GetStudentById(student.UserId, token), Times.Once);
        _repository.Verify(repository => repository.UpdateStudent(student, request.TopicIds, token), Times.Once);
    }

    [Fact]
    public async Task UpdateStudent_PropagatesLookupFailure()
    {
        var topicId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        var request = new UpdateStudent { DisplayName = "Updated", Program = "Engineering", TopicIds = [topicId] };
        using var cancellation = new CancellationTokenSource();
        var token = cancellation.Token;
        var original = new RepositoryException("dependency", new InvalidOperationException("Database failed"));

        _repository.Setup(repository => repository.GetStudentById(studentId, token))
            .ThrowsAsync(original);

        var service = CreateService();

        var exception = await Assert.ThrowsAsync<RepositoryException>(() => service.UpdateStudent(studentId, request, token));

        Assert.Same(original, exception);
        _repository.Verify(repository => repository.GetStudentById(studentId, token), Times.Once);
    }
}
