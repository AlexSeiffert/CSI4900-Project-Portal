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

    private StudentService CreateService() => new(_repository.Object);

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
        Assert.Equal(topic.Id, Assert.Single(result.TopicInterests).Id);
        _repository.Verify(repository => repository.GetStudents(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task GetStudents_WrapsRepositoryFailure()
    {
        using var cancellation = new CancellationTokenSource();
        var token = cancellation.Token;
        var original = new RepositoryException("dependency", new InvalidOperationException("Database failed"));

        _repository.Setup(repository => repository.GetStudents(token))
            .ThrowsAsync(original);

        var service = CreateService();

        var exception = await Assert.ThrowsAsync<ServiceException>(() => service.GetStudents(token));

        Assert.Equal(nameof(StudentService.GetStudents), exception.Operation);
        Assert.Equal(ErrorType.Unexpected, exception.Type);
        Assert.Same(original, exception.InnerException);
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
        Assert.Equal(topic.Id, Assert.Single(result.TopicInterests).Id);
        _repository.Verify(repository => repository.GetStudentById(student.UserId, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task GetStudentById_WrapsRepositoryFailure()
    {
        var studentId = Guid.NewGuid();
        using var cancellation = new CancellationTokenSource();
        var token = cancellation.Token;
        var original = new RepositoryException("dependency", new InvalidOperationException("Database failed"));

        _repository.Setup(repository => repository.GetStudentById(studentId, token))
            .ThrowsAsync(original);

        var service = CreateService();

        var exception = await Assert.ThrowsAsync<ServiceException>(() => service.GetStudentById(studentId, token));

        Assert.Equal(nameof(StudentService.GetStudentById), exception.Operation);
        Assert.Equal(ErrorType.Unexpected, exception.Type);
        Assert.Same(original, exception.InnerException);
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
    public async Task UpdateStudent_WrapsRepositoryFailure()
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

        var exception = await Assert.ThrowsAsync<ServiceException>(() => service.UpdateStudent(student.UserId, request, token));

        Assert.Equal(nameof(StudentService.UpdateStudent), exception.Operation);
        Assert.Equal(ErrorType.Unexpected, exception.Type);
        Assert.Same(original, exception.InnerException);
        _repository.Verify(repository => repository.GetStudentById(student.UserId, token), Times.Once);
        _repository.Verify(repository => repository.UpdateStudent(student, request.TopicIds, token), Times.Once);
    }

    [Fact]
    public async Task UpdateStudent_WrapsLookupFailure()
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

        var exception = await Assert.ThrowsAsync<ServiceException>(() => service.UpdateStudent(studentId, request, token));

        Assert.Equal(nameof(StudentService.UpdateStudent), exception.Operation);
        Assert.Equal(ErrorType.Unexpected, exception.Type);
        Assert.Same(original, exception.InnerException);
        _repository.Verify(repository => repository.GetStudentById(studentId, token), Times.Once);
    }

    [Fact]
    public async Task ValidateStudentExists_AcceptsExistingReferences()
    {
        var topic = new Topic { Id = Guid.NewGuid(), Name = "Databases" };
        var student = new Student { UserId = Guid.NewGuid(), DisplayName = "Student", Program = "CS", TopicInterests = [topic] };

        _repository.Setup(repository => repository.GetStudentById(student.UserId, CancellationToken.None))
            .ReturnsAsync(student);

        var service = CreateService();

        await service.ValidateStudentExists(student.UserId, CancellationToken.None);

        _repository.Verify(repository => repository.GetStudentById(student.UserId, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task ValidateStudentExists_WrapsRepositoryFailure()
    {
        var studentId = Guid.NewGuid();
        using var cancellation = new CancellationTokenSource();
        var original = new RepositoryException("dependency", new InvalidOperationException("Database failed"));

        _repository.Setup(repository => repository.GetStudentById(studentId, cancellation.Token))
            .ThrowsAsync(original);

        var service = CreateService();

        var exception = await Assert.ThrowsAsync<ServiceException>(() => service.ValidateStudentExists(studentId, cancellation.Token));

        Assert.Equal(nameof(StudentService.ValidateStudentExists), exception.Operation);
        Assert.Equal(ErrorType.Unexpected, exception.Type);
        Assert.Same(original, exception.InnerException);
        _repository.Verify(repository => repository.GetStudentById(studentId, cancellation.Token), Times.Once);
    }
}
