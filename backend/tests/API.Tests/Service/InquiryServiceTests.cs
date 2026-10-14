using API.DTOs.Request;
using API.Exceptions;
using API.Models;
using API.Tests.Support;

namespace API.Tests.Service;

public class InquiryServiceTests
{
    [Theory]
    [MemberData(nameof(ServiceFailureCases.All), MemberType = typeof(ServiceFailureCases))]
    public async Task GetInquiries_PreservesDependencyFailureAndClassification(string kind)
    {
        var c = new ServiceTestContext();
        using var cancellation = new CancellationTokenSource();
        var token = cancellation.Token;
        var original = ServiceFailureCases.Create(kind);
        c.Inquiries.Failure = original;
        c.Inquiries.FailureMethod = "GetInquiries";
        await ServiceFailureCases.AssertWrapped(() => c.InquiryService.GetInquiries(token), "GetInquiries", original);
        Assert.Equal(token, c.Inquiries.LastToken);
    }

    [Theory]
    [MemberData(nameof(ServiceFailureCases.All), MemberType = typeof(ServiceFailureCases))]
    public async Task GetInquiriesByStudentId_PreservesDependencyFailureAndClassification(string kind)
    {
        var c = new ServiceTestContext();
        using var cancellation = new CancellationTokenSource();
        var token = cancellation.Token;
        var original = ServiceFailureCases.Create(kind);
        c.Inquiries.Failure = original;
        c.Inquiries.FailureMethod = "GetInquiriesByStudentId";
        await ServiceFailureCases.AssertWrapped(() => c.InquiryService.GetInquiriesByStudentId(c.Student.UserId, token), "GetInquiriesByStudentId", original);
        Assert.Equal(token, c.Inquiries.LastToken);
    }

    [Theory]
    [MemberData(nameof(ServiceFailureCases.All), MemberType = typeof(ServiceFailureCases))]
    public async Task GetInquiriesBySupervisorId_PreservesDependencyFailureAndClassification(string kind)
    {
        var c = new ServiceTestContext();
        using var cancellation = new CancellationTokenSource();
        var token = cancellation.Token;
        var original = ServiceFailureCases.Create(kind);
        c.Inquiries.Failure = original;
        c.Inquiries.FailureMethod = "GetInquiriesBySupervisorId";
        await ServiceFailureCases.AssertWrapped(() => c.InquiryService.GetInquiriesBySupervisorId(c.Supervisor.UserId, token), "GetInquiriesBySupervisorId", original);
        Assert.Equal(token, c.Inquiries.LastToken);
    }

    [Theory]
    [MemberData(nameof(ServiceFailureCases.All), MemberType = typeof(ServiceFailureCases))]
    public async Task GetInquiriesByProjectId_PreservesDependencyFailureAndClassification(string kind)
    {
        var c = new ServiceTestContext();
        using var cancellation = new CancellationTokenSource();
        var token = cancellation.Token;
        var original = ServiceFailureCases.Create(kind);
        c.Inquiries.Failure = original;
        c.Inquiries.FailureMethod = "GetInquiriesByProjectId";
        await ServiceFailureCases.AssertWrapped(() => c.InquiryService.GetInquiriesByProjectId(c.Project.Id, token), "GetInquiriesByProjectId", original);
        Assert.Equal(token, c.Inquiries.LastToken);
    }

    [Theory]
    [MemberData(nameof(ServiceFailureCases.All), MemberType = typeof(ServiceFailureCases))]
    public async Task GetInquiryById_PreservesDependencyFailureAndClassification(string kind)
    {
        var c = new ServiceTestContext();
        using var cancellation = new CancellationTokenSource();
        var token = cancellation.Token;
        var original = ServiceFailureCases.Create(kind);
        c.Inquiries.Failure = original;
        c.Inquiries.FailureMethod = "GetInquiryById";
        await ServiceFailureCases.AssertWrapped(() => c.InquiryService.GetInquiryById(c.Inquiry.Id, token), "GetInquiryById", original);
        Assert.Equal(token, c.Inquiries.LastToken);
    }

    [Theory]
    [MemberData(nameof(ServiceFailureCases.All), MemberType = typeof(ServiceFailureCases))]
    public async Task CreateInquiry_PreservesDependencyFailureAndClassification(string kind)
    {
        var c = new ServiceTestContext();
        using var cancellation = new CancellationTokenSource();
        var token = cancellation.Token;
        var original = ServiceFailureCases.Create(kind);
        c.Inquiries.Failure = original;
        c.Inquiries.FailureMethod = "CreateInquiry";
        await ServiceFailureCases.AssertWrapped(() => c.InquiryService.CreateInquiry(c.NewInquiry(), token), "CreateInquiry", original);
        Assert.Equal(token, c.Inquiries.LastToken);
    }

    [Theory]
    [MemberData(nameof(ServiceFailureCases.All), MemberType = typeof(ServiceFailureCases))]
    public async Task UpdateInquiry_PreservesDependencyFailureAndClassification(string kind)
    {
        var c = new ServiceTestContext();
        using var cancellation = new CancellationTokenSource();
        var token = cancellation.Token;
        var original = ServiceFailureCases.Create(kind);
        c.Inquiries.Failure = original;
        c.Inquiries.FailureMethod = "UpdateInquiry";
        await ServiceFailureCases.AssertWrapped(() => c.InquiryService.UpdateInquiry(c.Inquiry.Id, new UpdateInquiry { Status = InquiryStatus.Responded }, token), "UpdateInquiry", original);
        Assert.Equal(token, c.Inquiries.LastToken);
    }

    [Fact]
    public async Task GetInquiries_ReturnsMatchingInquiryDtos()
    {
        var c = new ServiceTestContext();
        var result = Assert.Single(await c.InquiryService.GetInquiries());
        Assert.Equal(c.Inquiry.Id, result.Id);
        Assert.Equal(c.Inquiry.StudentId, result.StudentId);
        Assert.Equal(c.Inquiry.SupervisorId, result.SupervisorId);
        Assert.Equal(c.Inquiry.ProjectId, result.ProjectId);
        Assert.Equal(c.Inquiry.Message, result.Message);
        Assert.Equal("GetInquiries", Assert.Single(c.Inquiries.Calls));
    }

    [Fact]
    public async Task GetInquiriesByStudentId_ReturnsMatchingInquiryDtos()
    {
        var c = new ServiceTestContext();
        var result = Assert.Single(await c.InquiryService.GetInquiriesByStudentId(c.Student.UserId));
        Assert.Equal(c.Inquiry.Id, result.Id);
        Assert.Equal(c.Inquiry.StudentId, result.StudentId);
        Assert.Equal(c.Inquiry.SupervisorId, result.SupervisorId);
        Assert.Equal(c.Inquiry.ProjectId, result.ProjectId);
        Assert.Equal(c.Inquiry.Message, result.Message);
        Assert.Equal("GetInquiriesByStudentId", Assert.Single(c.Inquiries.Calls));
    }

    [Fact]
    public async Task GetInquiriesBySupervisorId_ReturnsMatchingInquiryDtos()
    {
        var c = new ServiceTestContext();
        var result = Assert.Single(await c.InquiryService.GetInquiriesBySupervisorId(c.Supervisor.UserId));
        Assert.Equal(c.Inquiry.Id, result.Id);
        Assert.Equal(c.Inquiry.StudentId, result.StudentId);
        Assert.Equal(c.Inquiry.SupervisorId, result.SupervisorId);
        Assert.Equal(c.Inquiry.ProjectId, result.ProjectId);
        Assert.Equal(c.Inquiry.Message, result.Message);
        Assert.Equal("GetInquiriesBySupervisorId", Assert.Single(c.Inquiries.Calls));
    }

    [Fact]
    public async Task GetInquiriesByProjectId_ReturnsMatchingInquiryDtos()
    {
        var c = new ServiceTestContext();
        var result = Assert.Single(await c.InquiryService.GetInquiriesByProjectId(c.Project.Id));
        Assert.Equal(c.Inquiry.Id, result.Id);
        Assert.Equal(c.Inquiry.StudentId, result.StudentId);
        Assert.Equal(c.Inquiry.SupervisorId, result.SupervisorId);
        Assert.Equal(c.Inquiry.ProjectId, result.ProjectId);
        Assert.Equal(c.Inquiry.Message, result.Message);
        Assert.Equal("GetInquiriesByProjectId", Assert.Single(c.Inquiries.Calls));
    }

    [Fact]
    public async Task GetInquiryById_ReturnsMappedInquiry()
    {
        var c = new ServiceTestContext();
        var result = await c.InquiryService.GetInquiryById(c.Inquiry.Id);
        Assert.Equal(c.Inquiry.Id, result.Id);
        Assert.Equal(c.Inquiry.Message, result.Message);
        Assert.Equal(c.Inquiry.Status, result.Status);
    }

    [Fact]
    public async Task CreateInquiry_PreservesReferencesMessageAndInitialStatus()
    {
        var c = new ServiceTestContext();
        var request = c.NewInquiry();
        var result = await c.InquiryService.CreateInquiry(request);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(request.StudentId, result.StudentId);
        Assert.Equal(request.SupervisorId, result.SupervisorId);
        Assert.Equal(request.ProjectId, result.ProjectId);
        Assert.Equal(request.Message, result.Message);
        Assert.Equal(InquiryStatus.Pending, result.Status);
        Assert.Equal("CreateInquiry", Assert.Single(c.Inquiries.Calls));
    }

    [Fact]
    public async Task CreateInquiry_AllowsGeneralInquiryWithoutProjectOrMessage()
    {
        var c = new ServiceTestContext();
        var request = c.NewInquiry();
        request.ProjectId = null;
        request.Message = null;
        var result = await c.InquiryService.CreateInquiry(request);
        Assert.Null(result.ProjectId);
        Assert.Null(result.Message);
        Assert.Empty(c.Projects.Calls);
    }

    [Fact]
    public async Task UpdateInquiry_ChangesOnlyStatus()
    {
        var c = new ServiceTestContext();
        var original = c.Inquiry;
        var result = await c.InquiryService.UpdateInquiry(original.Id, new UpdateInquiry { Status = InquiryStatus.Responded });
        Assert.Equal(InquiryStatus.Responded, result.Status);
        Assert.Equal(original.Id, result.Id);
        Assert.Equal(original.StudentId, result.StudentId);
        Assert.Equal(original.SupervisorId, result.SupervisorId);
        Assert.Equal(original.ProjectId, result.ProjectId);
        Assert.Equal("Original message", result.Message);
        Assert.Equal(original.CreatedAt, result.CreatedAt);
        Assert.Equal(new[] { "GetInquiryById", "UpdateInquiry" }, c.Inquiries.Calls);
    }

    [Fact]
    public async Task GetInquiryById_ThrowsForMissingInquiry()
    {
        var c = new ServiceTestContext();
        await ServiceFailureCases.AssertDomainFailure(() => c.InquiryService.GetInquiryById(Guid.NewGuid()), "GetInquiryById", ErrorType.NotFound);
        Assert.DoesNotContain("UpdateInquiry", c.Inquiries.Calls);
    }

    [Fact]
    public async Task UpdateInquiry_ThrowsForMissingInquiry()
    {
        var c = new ServiceTestContext();
        await ServiceFailureCases.AssertDomainFailure(() => c.InquiryService.UpdateInquiry(Guid.NewGuid(), new UpdateInquiry { Status = InquiryStatus.Viewed }), "UpdateInquiry", ErrorType.NotFound);
        Assert.DoesNotContain("UpdateInquiry", c.Inquiries.Calls);
    }

    [Theory]
    [InlineData(null)]
    [InlineData((InquiryStatus)999)]
    public async Task UpdateInquiry_RejectsInvalidStatusBeforeWriting(InquiryStatus? status)
    {
        var c = new ServiceTestContext();
        await ServiceFailureCases.AssertDomainFailure(() => c.InquiryService.UpdateInquiry(c.Inquiry.Id, new UpdateInquiry { Status = status }), "UpdateInquiry", ErrorType.Validation);
        Assert.Empty(c.Inquiries.Calls);
    }


    [Fact]
    public void Constructor_DependsOnlyOnOwnRepository()
    {
        var constructor = Assert.Single(typeof(API.Services.InquiryService).GetConstructors());
        var dependency = Assert.Single(constructor.GetParameters());
        Assert.Equal(typeof(API.Repositories.Interfaces.IInquiryRepository), dependency.ParameterType);
    }
}
