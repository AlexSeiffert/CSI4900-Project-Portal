using API.DTOs.Request;
using API.Exceptions;
using API.Models;
using API.Repositories.Interfaces;
using API.Services;
using Moq;

namespace API.Tests.Service;

public class InquiryServiceTests
{
    private readonly Mock<IInquiryRepository> _repository = new();

    private InquiryService CreateService() => new(_repository.Object);

    [Fact]
    public async Task GetInquiries_ReturnsMatchingInquiryDtos()
    {
        var studentId = Guid.NewGuid();
        var supervisorId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var inquiry = new Inquiry { Id = Guid.NewGuid(), StudentId = studentId, SupervisorId = supervisorId, ProjectId = projectId, Message = "Original message" };

        _repository.Setup(repository => repository.GetInquiries(CancellationToken.None))
            .ReturnsAsync(new List<Inquiry> { inquiry });

        var service = CreateService();

        var result = Assert.Single(await service.GetInquiries(CancellationToken.None));

        Assert.Equal(inquiry.Id, result.Id);
        Assert.Equal(inquiry.StudentId, result.StudentId);
        Assert.Equal(inquiry.SupervisorId, result.SupervisorId);
        Assert.Equal(inquiry.ProjectId, result.ProjectId);
        Assert.Equal(inquiry.Message, result.Message);
        _repository.Verify(repository => repository.GetInquiries(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task GetInquiries_WrapsRepositoryFailure()
    {
        using var cancellation = new CancellationTokenSource();
        var token = cancellation.Token;
        var original = new RepositoryException("dependency", new InvalidOperationException("Database failed"));

        _repository.Setup(repository => repository.GetInquiries(token))
            .ThrowsAsync(original);

        var service = CreateService();

        var exception = await Assert.ThrowsAsync<ServiceException>(() => service.GetInquiries(token));

        Assert.Equal(nameof(InquiryService.GetInquiries), exception.Operation);
        Assert.Equal(ErrorType.Unexpected, exception.Type);
        Assert.Same(original, exception.InnerException);
        _repository.Verify(repository => repository.GetInquiries(token), Times.Once);
    }

    [Fact]
    public async Task GetInquiriesByStudentId_ReturnsMatchingInquiryDtos()
    {
        var studentId = Guid.NewGuid();
        var supervisorId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var inquiry = new Inquiry { Id = Guid.NewGuid(), StudentId = studentId, SupervisorId = supervisorId, ProjectId = projectId, Message = "Original message" };

        _repository.Setup(repository => repository.GetInquiriesByStudentId(studentId, CancellationToken.None))
            .ReturnsAsync(new List<Inquiry> { inquiry });

        var service = CreateService();

        var result = Assert.Single(await service.GetInquiriesByStudentId(studentId, CancellationToken.None));

        Assert.Equal(inquiry.Id, result.Id);
        Assert.Equal(inquiry.StudentId, result.StudentId);
        Assert.Equal(inquiry.SupervisorId, result.SupervisorId);
        Assert.Equal(inquiry.ProjectId, result.ProjectId);
        Assert.Equal(inquiry.Message, result.Message);
        _repository.Verify(repository => repository.GetInquiriesByStudentId(studentId, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task GetInquiriesByStudentId_WrapsRepositoryFailure()
    {
        var studentId = Guid.NewGuid();
        using var cancellation = new CancellationTokenSource();
        var token = cancellation.Token;
        var original = new RepositoryException("dependency", new InvalidOperationException("Database failed"));

        _repository.Setup(repository => repository.GetInquiriesByStudentId(studentId, token))
            .ThrowsAsync(original);

        var service = CreateService();

        var exception = await Assert.ThrowsAsync<ServiceException>(() => service.GetInquiriesByStudentId(studentId, token));

        Assert.Equal(nameof(InquiryService.GetInquiriesByStudentId), exception.Operation);
        Assert.Equal(ErrorType.Unexpected, exception.Type);
        Assert.Same(original, exception.InnerException);
        _repository.Verify(repository => repository.GetInquiriesByStudentId(studentId, token), Times.Once);
    }

    [Fact]
    public async Task GetInquiriesBySupervisorId_ReturnsMatchingInquiryDtos()
    {
        var studentId = Guid.NewGuid();
        var supervisorId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var inquiry = new Inquiry { Id = Guid.NewGuid(), StudentId = studentId, SupervisorId = supervisorId, ProjectId = projectId, Message = "Original message" };

        _repository.Setup(repository => repository.GetInquiriesBySupervisorId(supervisorId, CancellationToken.None))
            .ReturnsAsync(new List<Inquiry> { inquiry });

        var service = CreateService();

        var result = Assert.Single(await service.GetInquiriesBySupervisorId(supervisorId, CancellationToken.None));

        Assert.Equal(inquiry.Id, result.Id);
        Assert.Equal(inquiry.StudentId, result.StudentId);
        Assert.Equal(inquiry.SupervisorId, result.SupervisorId);
        Assert.Equal(inquiry.ProjectId, result.ProjectId);
        Assert.Equal(inquiry.Message, result.Message);
        _repository.Verify(repository => repository.GetInquiriesBySupervisorId(supervisorId, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task GetInquiriesBySupervisorId_WrapsRepositoryFailure()
    {
        var supervisorId = Guid.NewGuid();
        using var cancellation = new CancellationTokenSource();
        var token = cancellation.Token;
        var original = new RepositoryException("dependency", new InvalidOperationException("Database failed"));

        _repository.Setup(repository => repository.GetInquiriesBySupervisorId(supervisorId, token))
            .ThrowsAsync(original);

        var service = CreateService();

        var exception = await Assert.ThrowsAsync<ServiceException>(() => service.GetInquiriesBySupervisorId(supervisorId, token));

        Assert.Equal(nameof(InquiryService.GetInquiriesBySupervisorId), exception.Operation);
        Assert.Equal(ErrorType.Unexpected, exception.Type);
        Assert.Same(original, exception.InnerException);
        _repository.Verify(repository => repository.GetInquiriesBySupervisorId(supervisorId, token), Times.Once);
    }

    [Fact]
    public async Task GetInquiriesByProjectId_ReturnsMatchingInquiryDtos()
    {
        var studentId = Guid.NewGuid();
        var supervisorId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var inquiry = new Inquiry { Id = Guid.NewGuid(), StudentId = studentId, SupervisorId = supervisorId, ProjectId = projectId, Message = "Original message" };

        _repository.Setup(repository => repository.GetInquiriesByProjectId(projectId, CancellationToken.None))
            .ReturnsAsync(new List<Inquiry> { inquiry });

        var service = CreateService();

        var result = Assert.Single(await service.GetInquiriesByProjectId(projectId, CancellationToken.None));

        Assert.Equal(inquiry.Id, result.Id);
        Assert.Equal(inquiry.StudentId, result.StudentId);
        Assert.Equal(inquiry.SupervisorId, result.SupervisorId);
        Assert.Equal(inquiry.ProjectId, result.ProjectId);
        Assert.Equal(inquiry.Message, result.Message);
        _repository.Verify(repository => repository.GetInquiriesByProjectId(projectId, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task GetInquiriesByProjectId_WrapsRepositoryFailure()
    {
        var projectId = Guid.NewGuid();
        using var cancellation = new CancellationTokenSource();
        var token = cancellation.Token;
        var original = new RepositoryException("dependency", new InvalidOperationException("Database failed"));

        _repository.Setup(repository => repository.GetInquiriesByProjectId(projectId, token))
            .ThrowsAsync(original);

        var service = CreateService();

        var exception = await Assert.ThrowsAsync<ServiceException>(() => service.GetInquiriesByProjectId(projectId, token));

        Assert.Equal(nameof(InquiryService.GetInquiriesByProjectId), exception.Operation);
        Assert.Equal(ErrorType.Unexpected, exception.Type);
        Assert.Same(original, exception.InnerException);
        _repository.Verify(repository => repository.GetInquiriesByProjectId(projectId, token), Times.Once);
    }

    [Fact]
    public async Task GetInquiryById_ReturnsMappedInquiry()
    {
        var studentId = Guid.NewGuid();
        var supervisorId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var inquiry = new Inquiry { Id = Guid.NewGuid(), StudentId = studentId, SupervisorId = supervisorId, ProjectId = projectId, Message = "Original message" };

        _repository.Setup(repository => repository.GetInquiryById(inquiry.Id, CancellationToken.None))
            .ReturnsAsync(inquiry);

        var service = CreateService();

        var result = await service.GetInquiryById(inquiry.Id, CancellationToken.None);

        Assert.Equal(inquiry.Id, result.Id);
        Assert.Equal(inquiry.Message, result.Message);
        Assert.Equal(inquiry.Status, result.Status);
        _repository.Verify(repository => repository.GetInquiryById(inquiry.Id, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task GetInquiryById_WrapsRepositoryFailure()
    {
        var inquiryId = Guid.NewGuid();
        using var cancellation = new CancellationTokenSource();
        var token = cancellation.Token;
        var original = new RepositoryException("dependency", new InvalidOperationException("Database failed"));

        _repository.Setup(repository => repository.GetInquiryById(inquiryId, token))
            .ThrowsAsync(original);

        var service = CreateService();

        var exception = await Assert.ThrowsAsync<ServiceException>(() => service.GetInquiryById(inquiryId, token));

        Assert.Equal(nameof(InquiryService.GetInquiryById), exception.Operation);
        Assert.Equal(ErrorType.Unexpected, exception.Type);
        Assert.Same(original, exception.InnerException);
        _repository.Verify(repository => repository.GetInquiryById(inquiryId, token), Times.Once);
    }

    [Fact]
    public async Task CreateInquiry_PreservesReferencesMessageAndInitialStatus()
    {
        var studentId = Guid.NewGuid();
        var supervisorId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var request = new CreateInquiry { StudentId = studentId, SupervisorId = supervisorId, ProjectId = projectId, Message = "Interested" };

        _repository.Setup(repository => repository.CreateInquiry(It.IsAny<Inquiry>(), CancellationToken.None))
            .ReturnsAsync((Inquiry value, CancellationToken cancellationToken) => value);

        var service = CreateService();

        var result = await service.CreateInquiry(request, CancellationToken.None);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(request.StudentId, result.StudentId);
        Assert.Equal(request.SupervisorId, result.SupervisorId);
        Assert.Equal(request.ProjectId, result.ProjectId);
        Assert.Equal(request.Message, result.Message);
        Assert.Equal(InquiryStatus.Pending, result.Status);
        _repository.Verify(repository => repository.CreateInquiry(It.IsAny<Inquiry>(), CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task CreateInquiry_WrapsRepositoryFailure()
    {
        var studentId = Guid.NewGuid();
        var supervisorId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var request = new CreateInquiry { StudentId = studentId, SupervisorId = supervisorId, ProjectId = projectId, Message = "Interested" };
        using var cancellation = new CancellationTokenSource();
        var token = cancellation.Token;
        var original = new RepositoryException("dependency", new InvalidOperationException("Database failed"));

        _repository.Setup(repository => repository.CreateInquiry(It.IsAny<Inquiry>(), token))
            .ThrowsAsync(original);

        var service = CreateService();

        var exception = await Assert.ThrowsAsync<ServiceException>(() => service.CreateInquiry(request, token));

        Assert.Equal(nameof(InquiryService.CreateInquiry), exception.Operation);
        Assert.Equal(ErrorType.Unexpected, exception.Type);
        Assert.Same(original, exception.InnerException);
        _repository.Verify(repository => repository.CreateInquiry(It.IsAny<Inquiry>(), token), Times.Once);
    }

    [Fact]
    public async Task UpdateInquiry_ChangesOnlyStatus()
    {
        var studentId = Guid.NewGuid();
        var supervisorId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var inquiry = new Inquiry { Id = Guid.NewGuid(), StudentId = studentId, SupervisorId = supervisorId, ProjectId = projectId, Message = "Original message" };
        var request = new UpdateInquiry { Status = InquiryStatus.Responded };
        var original = inquiry;

        _repository.Setup(repository => repository.GetInquiryById(original.Id, CancellationToken.None))
            .ReturnsAsync(inquiry);
        _repository.Setup(repository => repository.UpdateInquiry(inquiry, CancellationToken.None))
            .ReturnsAsync(inquiry);

        var service = CreateService();

        var result = await service.UpdateInquiry(original.Id, request, CancellationToken.None);

        Assert.Equal(InquiryStatus.Responded, result.Status);
        Assert.Equal(original.Id, result.Id);
        Assert.Equal(original.StudentId, result.StudentId);
        Assert.Equal(original.SupervisorId, result.SupervisorId);
        Assert.Equal(original.ProjectId, result.ProjectId);
        Assert.Equal("Original message", result.Message);
        Assert.Equal(original.CreatedAt, result.CreatedAt);
        _repository.Verify(repository => repository.GetInquiryById(original.Id, CancellationToken.None), Times.Once);
        _repository.Verify(repository => repository.UpdateInquiry(inquiry, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task UpdateInquiry_WrapsRepositoryFailure()
    {
        var studentId = Guid.NewGuid();
        var supervisorId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var inquiry = new Inquiry { Id = Guid.NewGuid(), StudentId = studentId, SupervisorId = supervisorId, ProjectId = projectId, Message = "Original message" };
        var request = new UpdateInquiry { Status = InquiryStatus.Responded };
        using var cancellation = new CancellationTokenSource();
        var token = cancellation.Token;
        var original = new RepositoryException("dependency", new InvalidOperationException("Database failed"));

        _repository.Setup(repository => repository.GetInquiryById(inquiry.Id, token))
            .ReturnsAsync(inquiry);
        _repository.Setup(repository => repository.UpdateInquiry(inquiry, token))
            .ThrowsAsync(original);

        var service = CreateService();

        var exception = await Assert.ThrowsAsync<ServiceException>(() => service.UpdateInquiry(inquiry.Id, request, token));

        Assert.Equal(nameof(InquiryService.UpdateInquiry), exception.Operation);
        Assert.Equal(ErrorType.Unexpected, exception.Type);
        Assert.Same(original, exception.InnerException);
        _repository.Verify(repository => repository.GetInquiryById(inquiry.Id, token), Times.Once);
        _repository.Verify(repository => repository.UpdateInquiry(inquiry, token), Times.Once);
    }

    [Fact]
    public async Task UpdateInquiry_WrapsLookupFailure()
    {
        var inquiryId = Guid.NewGuid();
        var request = new UpdateInquiry { Status = InquiryStatus.Responded };
        using var cancellation = new CancellationTokenSource();
        var token = cancellation.Token;
        var original = new RepositoryException("dependency", new InvalidOperationException("Database failed"));

        _repository.Setup(repository => repository.GetInquiryById(inquiryId, token))
            .ThrowsAsync(original);

        var service = CreateService();

        var exception = await Assert.ThrowsAsync<ServiceException>(() => service.UpdateInquiry(inquiryId, request, token));

        Assert.Equal(nameof(InquiryService.UpdateInquiry), exception.Operation);
        Assert.Equal(ErrorType.Unexpected, exception.Type);
        Assert.Same(original, exception.InnerException);
        _repository.Verify(repository => repository.GetInquiryById(inquiryId, token), Times.Once);
    }
}
