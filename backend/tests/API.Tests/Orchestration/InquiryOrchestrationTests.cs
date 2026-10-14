using API.DTOs;
using API.DTOs.Request;
using API.Exceptions;
using API.Orchestrations;
using API.Services.Interfaces;
using Moq;

namespace API.Tests.Orchestration;

public class InquiryOrchestrationTests
{
    private readonly Mock<IInquiryService> _inquiries = new();
    private readonly Mock<IStudentService> _students = new();
    private readonly Mock<ISupervisorService> _supervisors = new();
    private readonly Mock<IProjectService> _projects = new();

    private InquiryOrchestration CreateOrchestration() => new(_inquiries.Object, _students.Object, _supervisors.Object, _projects.Object);

    [Fact]
    public async Task CreateInquiry_CallsServicesAndReturnsResult()
    {
        var request = new CreateInquiry { StudentId = Guid.NewGuid(), SupervisorId = Guid.NewGuid(), ProjectId = Guid.NewGuid() };
        var expected = new Inquiry { Id = Guid.NewGuid() };

        _students.Setup(service => service.ValidateStudentExists(request.StudentId, CancellationToken.None))
            .Returns(Task.CompletedTask);
        _supervisors.Setup(service => service.ValidateSupervisorExists(request.SupervisorId, CancellationToken.None))
            .Returns(Task.CompletedTask);
        _projects.Setup(service => service.ValidateProjectSupervisor(request.ProjectId, request.SupervisorId, CancellationToken.None))
            .Returns(Task.CompletedTask);
        _inquiries.Setup(service => service.CreateInquiry(request, CancellationToken.None))
            .ReturnsAsync(expected);

        var orchestration = CreateOrchestration();

        var result = await orchestration.CreateInquiry(request, CancellationToken.None);

        Assert.Same(expected, result);
        _students.Verify(service => service.ValidateStudentExists(request.StudentId, CancellationToken.None), Times.Once);
        _supervisors.Verify(service => service.ValidateSupervisorExists(request.SupervisorId, CancellationToken.None), Times.Once);
        _projects.Verify(service => service.ValidateProjectSupervisor(request.ProjectId, request.SupervisorId, CancellationToken.None), Times.Once);
        _inquiries.Verify(service => service.CreateInquiry(request, CancellationToken.None), Times.Once);
    }

    [Theory]
    [InlineData(ErrorType.Validation)]
    [InlineData(ErrorType.NotFound)]
    [InlineData(ErrorType.Conflict)]
    [InlineData(ErrorType.Cancelled)]
    [InlineData(ErrorType.Unexpected)]
    public async Task CreateInquiry_WrapsStudentsServiceException(ErrorType type)
    {
        var request = new CreateInquiry { StudentId = Guid.NewGuid(), SupervisorId = Guid.NewGuid(), ProjectId = Guid.NewGuid() };
        var original = new ServiceException("ValidateStudentExists", type, "Service failed", new InvalidOperationException("Original cause"));

        _students.Setup(service => service.ValidateStudentExists(request.StudentId, CancellationToken.None))
            .ThrowsAsync(original);

        var orchestration = CreateOrchestration();

        var exception = await Assert.ThrowsAsync<OrchestrationException>(() => orchestration.CreateInquiry(request, CancellationToken.None));

        Assert.Equal(nameof(InquiryOrchestration.CreateInquiry), exception.Operation);
        Assert.Equal(type, exception.Type);
        Assert.Same(original, exception.InnerException);
        Assert.Equal(original.Message, exception.Message);
        _students.Verify(service => service.ValidateStudentExists(request.StudentId, CancellationToken.None), Times.Once);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CreateInquiry_WrapsStudentsUnexpectedFailureAndCancellation(bool cancelled)
    {
        var request = new CreateInquiry { StudentId = Guid.NewGuid(), SupervisorId = Guid.NewGuid(), ProjectId = Guid.NewGuid() };
        Exception original = cancelled
            ? new OperationCanceledException()
            : new InvalidOperationException("Unexpected failure");

        _students.Setup(service => service.ValidateStudentExists(request.StudentId, CancellationToken.None))
            .ThrowsAsync(original);

        var orchestration = CreateOrchestration();

        var exception = await Assert.ThrowsAsync<OrchestrationException>(() => orchestration.CreateInquiry(request, CancellationToken.None));

        Assert.Equal(nameof(InquiryOrchestration.CreateInquiry), exception.Operation);
        Assert.Equal(cancelled ? ErrorType.Cancelled : ErrorType.Unexpected, exception.Type);
        Assert.Same(original, exception.InnerException);
        _students.Verify(service => service.ValidateStudentExists(request.StudentId, CancellationToken.None), Times.Once);
    }

    [Theory]
    [InlineData(ErrorType.Validation)]
    [InlineData(ErrorType.NotFound)]
    [InlineData(ErrorType.Conflict)]
    [InlineData(ErrorType.Cancelled)]
    [InlineData(ErrorType.Unexpected)]
    public async Task CreateInquiry_WrapsSupervisorsServiceException(ErrorType type)
    {
        var request = new CreateInquiry { StudentId = Guid.NewGuid(), SupervisorId = Guid.NewGuid(), ProjectId = Guid.NewGuid() };
        var original = new ServiceException("ValidateSupervisorExists", type, "Service failed", new InvalidOperationException("Original cause"));

        _students.Setup(service => service.ValidateStudentExists(request.StudentId, CancellationToken.None))
            .Returns(Task.CompletedTask);
        _supervisors.Setup(service => service.ValidateSupervisorExists(request.SupervisorId, CancellationToken.None))
            .ThrowsAsync(original);

        var orchestration = CreateOrchestration();

        var exception = await Assert.ThrowsAsync<OrchestrationException>(() => orchestration.CreateInquiry(request, CancellationToken.None));

        Assert.Equal(nameof(InquiryOrchestration.CreateInquiry), exception.Operation);
        Assert.Equal(type, exception.Type);
        Assert.Same(original, exception.InnerException);
        Assert.Equal(original.Message, exception.Message);
        _students.Verify(service => service.ValidateStudentExists(request.StudentId, CancellationToken.None), Times.Once);
        _supervisors.Verify(service => service.ValidateSupervisorExists(request.SupervisorId, CancellationToken.None), Times.Once);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CreateInquiry_WrapsSupervisorsUnexpectedFailureAndCancellation(bool cancelled)
    {
        var request = new CreateInquiry { StudentId = Guid.NewGuid(), SupervisorId = Guid.NewGuid(), ProjectId = Guid.NewGuid() };
        Exception original = cancelled
            ? new OperationCanceledException()
            : new InvalidOperationException("Unexpected failure");

        _students.Setup(service => service.ValidateStudentExists(request.StudentId, CancellationToken.None))
            .Returns(Task.CompletedTask);
        _supervisors.Setup(service => service.ValidateSupervisorExists(request.SupervisorId, CancellationToken.None))
            .ThrowsAsync(original);

        var orchestration = CreateOrchestration();

        var exception = await Assert.ThrowsAsync<OrchestrationException>(() => orchestration.CreateInquiry(request, CancellationToken.None));

        Assert.Equal(nameof(InquiryOrchestration.CreateInquiry), exception.Operation);
        Assert.Equal(cancelled ? ErrorType.Cancelled : ErrorType.Unexpected, exception.Type);
        Assert.Same(original, exception.InnerException);
        _students.Verify(service => service.ValidateStudentExists(request.StudentId, CancellationToken.None), Times.Once);
        _supervisors.Verify(service => service.ValidateSupervisorExists(request.SupervisorId, CancellationToken.None), Times.Once);
    }

    [Theory]
    [InlineData(ErrorType.Validation)]
    [InlineData(ErrorType.NotFound)]
    [InlineData(ErrorType.Conflict)]
    [InlineData(ErrorType.Cancelled)]
    [InlineData(ErrorType.Unexpected)]
    public async Task CreateInquiry_WrapsProjectsServiceException(ErrorType type)
    {
        var request = new CreateInquiry { StudentId = Guid.NewGuid(), SupervisorId = Guid.NewGuid(), ProjectId = Guid.NewGuid() };
        var original = new ServiceException("ValidateProjectSupervisor", type, "Service failed", new InvalidOperationException("Original cause"));

        _students.Setup(service => service.ValidateStudentExists(request.StudentId, CancellationToken.None))
            .Returns(Task.CompletedTask);
        _supervisors.Setup(service => service.ValidateSupervisorExists(request.SupervisorId, CancellationToken.None))
            .Returns(Task.CompletedTask);
        _projects.Setup(service => service.ValidateProjectSupervisor(request.ProjectId, request.SupervisorId, CancellationToken.None))
            .ThrowsAsync(original);

        var orchestration = CreateOrchestration();

        var exception = await Assert.ThrowsAsync<OrchestrationException>(() => orchestration.CreateInquiry(request, CancellationToken.None));

        Assert.Equal(nameof(InquiryOrchestration.CreateInquiry), exception.Operation);
        Assert.Equal(type, exception.Type);
        Assert.Same(original, exception.InnerException);
        Assert.Equal(original.Message, exception.Message);
        _students.Verify(service => service.ValidateStudentExists(request.StudentId, CancellationToken.None), Times.Once);
        _supervisors.Verify(service => service.ValidateSupervisorExists(request.SupervisorId, CancellationToken.None), Times.Once);
        _projects.Verify(service => service.ValidateProjectSupervisor(request.ProjectId, request.SupervisorId, CancellationToken.None), Times.Once);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CreateInquiry_WrapsProjectsUnexpectedFailureAndCancellation(bool cancelled)
    {
        var request = new CreateInquiry { StudentId = Guid.NewGuid(), SupervisorId = Guid.NewGuid(), ProjectId = Guid.NewGuid() };
        Exception original = cancelled
            ? new OperationCanceledException()
            : new InvalidOperationException("Unexpected failure");

        _students.Setup(service => service.ValidateStudentExists(request.StudentId, CancellationToken.None))
            .Returns(Task.CompletedTask);
        _supervisors.Setup(service => service.ValidateSupervisorExists(request.SupervisorId, CancellationToken.None))
            .Returns(Task.CompletedTask);
        _projects.Setup(service => service.ValidateProjectSupervisor(request.ProjectId, request.SupervisorId, CancellationToken.None))
            .ThrowsAsync(original);

        var orchestration = CreateOrchestration();

        var exception = await Assert.ThrowsAsync<OrchestrationException>(() => orchestration.CreateInquiry(request, CancellationToken.None));

        Assert.Equal(nameof(InquiryOrchestration.CreateInquiry), exception.Operation);
        Assert.Equal(cancelled ? ErrorType.Cancelled : ErrorType.Unexpected, exception.Type);
        Assert.Same(original, exception.InnerException);
        _students.Verify(service => service.ValidateStudentExists(request.StudentId, CancellationToken.None), Times.Once);
        _supervisors.Verify(service => service.ValidateSupervisorExists(request.SupervisorId, CancellationToken.None), Times.Once);
        _projects.Verify(service => service.ValidateProjectSupervisor(request.ProjectId, request.SupervisorId, CancellationToken.None), Times.Once);
    }

    [Theory]
    [InlineData(ErrorType.Validation)]
    [InlineData(ErrorType.NotFound)]
    [InlineData(ErrorType.Conflict)]
    [InlineData(ErrorType.Cancelled)]
    [InlineData(ErrorType.Unexpected)]
    public async Task CreateInquiry_WrapsInquiriesServiceException(ErrorType type)
    {
        var request = new CreateInquiry { StudentId = Guid.NewGuid(), SupervisorId = Guid.NewGuid(), ProjectId = Guid.NewGuid() };
        var original = new ServiceException("CreateInquiry", type, "Service failed", new InvalidOperationException("Original cause"));

        _students.Setup(service => service.ValidateStudentExists(request.StudentId, CancellationToken.None))
            .Returns(Task.CompletedTask);
        _supervisors.Setup(service => service.ValidateSupervisorExists(request.SupervisorId, CancellationToken.None))
            .Returns(Task.CompletedTask);
        _projects.Setup(service => service.ValidateProjectSupervisor(request.ProjectId, request.SupervisorId, CancellationToken.None))
            .Returns(Task.CompletedTask);
        _inquiries.Setup(service => service.CreateInquiry(request, CancellationToken.None))
            .ThrowsAsync(original);

        var orchestration = CreateOrchestration();

        var exception = await Assert.ThrowsAsync<OrchestrationException>(() => orchestration.CreateInquiry(request, CancellationToken.None));

        Assert.Equal(nameof(InquiryOrchestration.CreateInquiry), exception.Operation);
        Assert.Equal(type, exception.Type);
        Assert.Same(original, exception.InnerException);
        Assert.Equal(original.Message, exception.Message);
        _students.Verify(service => service.ValidateStudentExists(request.StudentId, CancellationToken.None), Times.Once);
        _supervisors.Verify(service => service.ValidateSupervisorExists(request.SupervisorId, CancellationToken.None), Times.Once);
        _projects.Verify(service => service.ValidateProjectSupervisor(request.ProjectId, request.SupervisorId, CancellationToken.None), Times.Once);
        _inquiries.Verify(service => service.CreateInquiry(request, CancellationToken.None), Times.Once);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CreateInquiry_WrapsInquiriesUnexpectedFailureAndCancellation(bool cancelled)
    {
        var request = new CreateInquiry { StudentId = Guid.NewGuid(), SupervisorId = Guid.NewGuid(), ProjectId = Guid.NewGuid() };
        Exception original = cancelled
            ? new OperationCanceledException()
            : new InvalidOperationException("Unexpected failure");

        _students.Setup(service => service.ValidateStudentExists(request.StudentId, CancellationToken.None))
            .Returns(Task.CompletedTask);
        _supervisors.Setup(service => service.ValidateSupervisorExists(request.SupervisorId, CancellationToken.None))
            .Returns(Task.CompletedTask);
        _projects.Setup(service => service.ValidateProjectSupervisor(request.ProjectId, request.SupervisorId, CancellationToken.None))
            .Returns(Task.CompletedTask);
        _inquiries.Setup(service => service.CreateInquiry(request, CancellationToken.None))
            .ThrowsAsync(original);

        var orchestration = CreateOrchestration();

        var exception = await Assert.ThrowsAsync<OrchestrationException>(() => orchestration.CreateInquiry(request, CancellationToken.None));

        Assert.Equal(nameof(InquiryOrchestration.CreateInquiry), exception.Operation);
        Assert.Equal(cancelled ? ErrorType.Cancelled : ErrorType.Unexpected, exception.Type);
        Assert.Same(original, exception.InnerException);
        _students.Verify(service => service.ValidateStudentExists(request.StudentId, CancellationToken.None), Times.Once);
        _supervisors.Verify(service => service.ValidateSupervisorExists(request.SupervisorId, CancellationToken.None), Times.Once);
        _projects.Verify(service => service.ValidateProjectSupervisor(request.ProjectId, request.SupervisorId, CancellationToken.None), Times.Once);
        _inquiries.Verify(service => service.CreateInquiry(request, CancellationToken.None), Times.Once);
    }
}
