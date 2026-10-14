using API.DTOs.Request;
using API.Exceptions;
using API.Models;
using API.Tests.Support;

namespace API.Tests.Orchestration;

public class InquiryOrchestrationTests
{
    [Fact]
    public async Task CreateInquiry_RejectsUnknownStudentBeforeWriting()
    {
        var c = new ServiceTestContext();
        var request = c.NewInquiry();
        request.StudentId = Guid.NewGuid();
        await OrchestrationFailureCases.AssertDomainFailure(() => c.InquiryOrchestration.CreateInquiry(request), "CreateInquiry", ErrorKind.Validation);
        Assert.Empty(c.Inquiries.Calls);
    }

    [Fact]
    public async Task CreateInquiry_RejectsUnknownSupervisorBeforeWriting()
    {
        var c = new ServiceTestContext();
        var request = c.NewInquiry();
        request.SupervisorId = Guid.NewGuid();
        await OrchestrationFailureCases.AssertDomainFailure(() => c.InquiryOrchestration.CreateInquiry(request), "CreateInquiry", ErrorKind.Validation);
        Assert.Empty(c.Inquiries.Calls);
    }

    [Fact]
    public async Task CreateInquiry_RejectsUnknownProjectBeforeWriting()
    {
        var c = new ServiceTestContext();
        var request = c.NewInquiry();
        request.ProjectId = Guid.NewGuid();
        await OrchestrationFailureCases.AssertDomainFailure(() => c.InquiryOrchestration.CreateInquiry(request), "CreateInquiry", ErrorKind.Validation);
        Assert.Empty(c.Inquiries.Calls);
    }

    [Fact]
    public async Task CreateInquiry_RejectsDifferentProjectOwnerBeforeWriting()
    {
        var c = new ServiceTestContext();
        var request = c.NewInquiry();
        c.Project.SupervisorId = Guid.NewGuid();
        await OrchestrationFailureCases.AssertDomainFailure(() => c.InquiryOrchestration.CreateInquiry(request), "CreateInquiry", ErrorKind.Validation);
        Assert.Empty(c.Inquiries.Calls);
    }

    [Fact]
    public async Task CreateInquiry_WrapsStudentsLookupFailureBeforeWriting()
    {
        var c = new ServiceTestContext();
        var original = new RepositoryException("lookup", new InvalidOperationException("Database failed"));
        c.Students.Failure = original;
        await OrchestrationFailureCases.AssertRepositoryFailure(() => c.InquiryOrchestration.CreateInquiry(c.NewInquiry()), "CreateInquiry", original);
        Assert.DoesNotContain("CreateInquiry", c.Inquiries.Calls);
    }

    [Fact]
    public async Task CreateInquiry_WrapsSupervisorsLookupFailureBeforeWriting()
    {
        var c = new ServiceTestContext();
        var original = new RepositoryException("lookup", new InvalidOperationException("Database failed"));
        c.Supervisors.Failure = original;
        await OrchestrationFailureCases.AssertRepositoryFailure(() => c.InquiryOrchestration.CreateInquiry(c.NewInquiry()), "CreateInquiry", original);
        Assert.DoesNotContain("CreateInquiry", c.Inquiries.Calls);
    }

    [Fact]
    public async Task CreateInquiry_WrapsProjectsLookupFailureBeforeWriting()
    {
        var c = new ServiceTestContext();
        var original = new RepositoryException("lookup", new InvalidOperationException("Database failed"));
        c.Projects.Failure = original;
        await OrchestrationFailureCases.AssertRepositoryFailure(() => c.InquiryOrchestration.CreateInquiry(c.NewInquiry()), "CreateInquiry", original);
        Assert.DoesNotContain("CreateInquiry", c.Inquiries.Calls);
    }

    [Fact]
    public async Task CreateInquiry_CoordinatesServicesInOrderAndForwardsArguments()
    {
        var c = new OrchestrationTestContext();
        using var cancellation = new CancellationTokenSource();
        var request = c.NewInquiry();
        var result = await c.InquiryOrchestration.CreateInquiry(request, cancellation.Token);
        Assert.Same(c.Inquiries.Item, result);
        Assert.Equal(new[] { "Students.ValidateStudentExists", "Supervisors.ValidateSupervisorExists", "Projects.ValidateProjectSupervisor", "Inquiries.CreateInquiry" }, c.Calls);
        Assert.Equal(cancellation.Token, c.Students.LastToken);
        Assert.Equal(request.StudentId, c.Students.LastArguments[0]);
        Assert.Equal(cancellation.Token, c.Supervisors.LastToken);
        Assert.Equal(request.SupervisorId, c.Supervisors.LastArguments[0]);
        Assert.Equal(cancellation.Token, c.Projects.LastToken);
        Assert.Equal(request.ProjectId, c.Projects.LastArguments[0]);
        Assert.Equal(request.SupervisorId, c.Projects.LastArguments[1]);
        Assert.Equal(cancellation.Token, c.Inquiries.LastToken);
        Assert.Equal(request, c.Inquiries.LastArguments[0]);
    }

    [Theory]
    [InlineData(ErrorKind.Validation)]
    [InlineData(ErrorKind.NotFound)]
    [InlineData(ErrorKind.Conflict)]
    [InlineData(ErrorKind.Cancelled)]
    [InlineData(ErrorKind.Unexpected)]
    public async Task CreateInquiry_WrapsStudentsFailureAndStopsRemainingCalls(ErrorKind kind)
    {
        var c = new OrchestrationTestContext();
        using var cancellation = new CancellationTokenSource();
        var request = c.NewInquiry();
        var root = new InvalidOperationException("Original failure");
        var original = new ServiceException("ValidateStudentExists", kind, "Service failed", root);
        c.Students.Failure = original;
        var exception = await Assert.ThrowsAsync<OrchestrationException>(() => c.InquiryOrchestration.CreateInquiry(request, cancellation.Token));
        Assert.Equal("CreateInquiry", exception.Operation);
        Assert.Equal(kind, exception.Kind);
        Assert.Same(original, exception.InnerException);
        Assert.Same(root, exception.InnerException!.InnerException);
        Assert.Equal(new[] { "Students.ValidateStudentExists" }, c.Calls);
        Assert.Equal(cancellation.Token, c.Students.LastToken);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CreateInquiry_WrapsStudentsUnexpectedFailureAndCancellation(bool cancelled)
    {
        var c = new OrchestrationTestContext();
        using var cancellation = new CancellationTokenSource();
        var request = c.NewInquiry();
        Exception original = cancelled ? new OperationCanceledException(cancellation.Token) : new InvalidOperationException("Unexpected failure");
        c.Students.Failure = original;
        var exception = await Assert.ThrowsAsync<OrchestrationException>(() => c.InquiryOrchestration.CreateInquiry(request, cancellation.Token));
        Assert.Equal("CreateInquiry", exception.Operation);
        Assert.Equal(cancelled ? ErrorKind.Cancelled : ErrorKind.Unexpected, exception.Kind);
        Assert.Same(original, exception.InnerException);
        Assert.Equal(new[] { "Students.ValidateStudentExists" }, c.Calls);
    }

    [Theory]
    [InlineData(ErrorKind.Validation)]
    [InlineData(ErrorKind.NotFound)]
    [InlineData(ErrorKind.Conflict)]
    [InlineData(ErrorKind.Cancelled)]
    [InlineData(ErrorKind.Unexpected)]
    public async Task CreateInquiry_WrapsSupervisorsFailureAndStopsRemainingCalls(ErrorKind kind)
    {
        var c = new OrchestrationTestContext();
        using var cancellation = new CancellationTokenSource();
        var request = c.NewInquiry();
        var root = new InvalidOperationException("Original failure");
        var original = new ServiceException("ValidateSupervisorExists", kind, "Service failed", root);
        c.Supervisors.Failure = original;
        var exception = await Assert.ThrowsAsync<OrchestrationException>(() => c.InquiryOrchestration.CreateInquiry(request, cancellation.Token));
        Assert.Equal("CreateInquiry", exception.Operation);
        Assert.Equal(kind, exception.Kind);
        Assert.Same(original, exception.InnerException);
        Assert.Same(root, exception.InnerException!.InnerException);
        Assert.Equal(new[] { "Students.ValidateStudentExists", "Supervisors.ValidateSupervisorExists" }, c.Calls);
        Assert.Equal(cancellation.Token, c.Supervisors.LastToken);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CreateInquiry_WrapsSupervisorsUnexpectedFailureAndCancellation(bool cancelled)
    {
        var c = new OrchestrationTestContext();
        using var cancellation = new CancellationTokenSource();
        var request = c.NewInquiry();
        Exception original = cancelled ? new OperationCanceledException(cancellation.Token) : new InvalidOperationException("Unexpected failure");
        c.Supervisors.Failure = original;
        var exception = await Assert.ThrowsAsync<OrchestrationException>(() => c.InquiryOrchestration.CreateInquiry(request, cancellation.Token));
        Assert.Equal("CreateInquiry", exception.Operation);
        Assert.Equal(cancelled ? ErrorKind.Cancelled : ErrorKind.Unexpected, exception.Kind);
        Assert.Same(original, exception.InnerException);
        Assert.Equal(new[] { "Students.ValidateStudentExists", "Supervisors.ValidateSupervisorExists" }, c.Calls);
    }

    [Theory]
    [InlineData(ErrorKind.Validation)]
    [InlineData(ErrorKind.NotFound)]
    [InlineData(ErrorKind.Conflict)]
    [InlineData(ErrorKind.Cancelled)]
    [InlineData(ErrorKind.Unexpected)]
    public async Task CreateInquiry_WrapsProjectsFailureAndStopsRemainingCalls(ErrorKind kind)
    {
        var c = new OrchestrationTestContext();
        using var cancellation = new CancellationTokenSource();
        var request = c.NewInquiry();
        var root = new InvalidOperationException("Original failure");
        var original = new ServiceException("ValidateProjectSupervisor", kind, "Service failed", root);
        c.Projects.Failure = original;
        var exception = await Assert.ThrowsAsync<OrchestrationException>(() => c.InquiryOrchestration.CreateInquiry(request, cancellation.Token));
        Assert.Equal("CreateInquiry", exception.Operation);
        Assert.Equal(kind, exception.Kind);
        Assert.Same(original, exception.InnerException);
        Assert.Same(root, exception.InnerException!.InnerException);
        Assert.Equal(new[] { "Students.ValidateStudentExists", "Supervisors.ValidateSupervisorExists", "Projects.ValidateProjectSupervisor" }, c.Calls);
        Assert.Equal(cancellation.Token, c.Projects.LastToken);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CreateInquiry_WrapsProjectsUnexpectedFailureAndCancellation(bool cancelled)
    {
        var c = new OrchestrationTestContext();
        using var cancellation = new CancellationTokenSource();
        var request = c.NewInquiry();
        Exception original = cancelled ? new OperationCanceledException(cancellation.Token) : new InvalidOperationException("Unexpected failure");
        c.Projects.Failure = original;
        var exception = await Assert.ThrowsAsync<OrchestrationException>(() => c.InquiryOrchestration.CreateInquiry(request, cancellation.Token));
        Assert.Equal("CreateInquiry", exception.Operation);
        Assert.Equal(cancelled ? ErrorKind.Cancelled : ErrorKind.Unexpected, exception.Kind);
        Assert.Same(original, exception.InnerException);
        Assert.Equal(new[] { "Students.ValidateStudentExists", "Supervisors.ValidateSupervisorExists", "Projects.ValidateProjectSupervisor" }, c.Calls);
    }

    [Theory]
    [InlineData(ErrorKind.Validation)]
    [InlineData(ErrorKind.NotFound)]
    [InlineData(ErrorKind.Conflict)]
    [InlineData(ErrorKind.Cancelled)]
    [InlineData(ErrorKind.Unexpected)]
    public async Task CreateInquiry_WrapsInquiriesFailureAndStopsRemainingCalls(ErrorKind kind)
    {
        var c = new OrchestrationTestContext();
        using var cancellation = new CancellationTokenSource();
        var request = c.NewInquiry();
        var root = new InvalidOperationException("Original failure");
        var original = new ServiceException("CreateInquiry", kind, "Service failed", root);
        c.Inquiries.Failure = original;
        var exception = await Assert.ThrowsAsync<OrchestrationException>(() => c.InquiryOrchestration.CreateInquiry(request, cancellation.Token));
        Assert.Equal("CreateInquiry", exception.Operation);
        Assert.Equal(kind, exception.Kind);
        Assert.Same(original, exception.InnerException);
        Assert.Same(root, exception.InnerException!.InnerException);
        Assert.Equal(new[] { "Students.ValidateStudentExists", "Supervisors.ValidateSupervisorExists", "Projects.ValidateProjectSupervisor", "Inquiries.CreateInquiry" }, c.Calls);
        Assert.Equal(cancellation.Token, c.Inquiries.LastToken);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CreateInquiry_WrapsInquiriesUnexpectedFailureAndCancellation(bool cancelled)
    {
        var c = new OrchestrationTestContext();
        using var cancellation = new CancellationTokenSource();
        var request = c.NewInquiry();
        Exception original = cancelled ? new OperationCanceledException(cancellation.Token) : new InvalidOperationException("Unexpected failure");
        c.Inquiries.Failure = original;
        var exception = await Assert.ThrowsAsync<OrchestrationException>(() => c.InquiryOrchestration.CreateInquiry(request, cancellation.Token));
        Assert.Equal("CreateInquiry", exception.Operation);
        Assert.Equal(cancelled ? ErrorKind.Cancelled : ErrorKind.Unexpected, exception.Kind);
        Assert.Same(original, exception.InnerException);
        Assert.Equal(new[] { "Students.ValidateStudentExists", "Supervisors.ValidateSupervisorExists", "Projects.ValidateProjectSupervisor", "Inquiries.CreateInquiry" }, c.Calls);
    }

    [Fact]
    public void Constructor_DependsOnlyOnTheServicesItCoordinates()
    {
        var constructor = Assert.Single(typeof(API.Orchestrations.InquiryOrchestration).GetConstructors());
        var dependencies = constructor.GetParameters().Select(parameter => parameter.ParameterType).ToHashSet();
        Assert.True(dependencies.SetEquals(new[] { typeof(API.Services.Interfaces.IStudentService), typeof(API.Services.Interfaces.ISupervisorService), typeof(API.Services.Interfaces.IProjectService), typeof(API.Services.Interfaces.IInquiryService) }));
    }
}
