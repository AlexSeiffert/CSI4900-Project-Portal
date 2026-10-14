using API.DTOs.Request;
using API.Exceptions;
using API.Models;
using API.Tests.Support;

namespace API.Tests.Service;

public class StudentServiceTests
{
    [Theory]
    [MemberData(nameof(ServiceFailureCases.All), MemberType = typeof(ServiceFailureCases))]
    public async Task GetStudents_PreservesDependencyFailureAndClassification(string kind)
    {
        var c = new ServiceTestContext();
        using var cancellation = new CancellationTokenSource();
        var token = cancellation.Token;
        var original = ServiceFailureCases.Create(kind);
        c.Students.Failure = original;
        c.Students.FailureMethod = "GetStudents";
        await ServiceFailureCases.AssertWrapped(() => c.StudentService.GetStudents(token), "GetStudents", original);
        Assert.Equal(token, c.Students.LastToken);
    }

    [Theory]
    [MemberData(nameof(ServiceFailureCases.All), MemberType = typeof(ServiceFailureCases))]
    public async Task GetStudentById_PreservesDependencyFailureAndClassification(string kind)
    {
        var c = new ServiceTestContext();
        using var cancellation = new CancellationTokenSource();
        var token = cancellation.Token;
        var original = ServiceFailureCases.Create(kind);
        c.Students.Failure = original;
        c.Students.FailureMethod = "GetStudentById";
        await ServiceFailureCases.AssertWrapped(() => c.StudentService.GetStudentById(c.Student.UserId, token), "GetStudentById", original);
        Assert.Equal(token, c.Students.LastToken);
    }

    [Theory]
    [MemberData(nameof(ServiceFailureCases.All), MemberType = typeof(ServiceFailureCases))]
    public async Task UpdateStudent_PreservesDependencyFailureAndClassification(string kind)
    {
        var c = new ServiceTestContext();
        using var cancellation = new CancellationTokenSource();
        var token = cancellation.Token;
        var original = ServiceFailureCases.Create(kind);
        c.Students.Failure = original;
        c.Students.FailureMethod = "UpdateStudent";
        await ServiceFailureCases.AssertWrapped(() => c.StudentService.UpdateStudent(c.Student.UserId, c.StudentUpdate(), token), "UpdateStudent", original);
        Assert.Equal(token, c.Students.LastToken);
    }

    [Fact]
    public async Task GetStudents_MapsProfilesAndTopics()
    {
        var c = new ServiceTestContext();
        var result = Assert.Single(await c.StudentService.GetStudents());
        Assert.Equal(c.Student.UserId, result.UserId);
        Assert.Equal(c.Student.DisplayName, result.DisplayName);
        Assert.Equal(c.Topic.Id, Assert.Single(result.TopicInterests).Id);
    }

    [Fact]
    public async Task GetStudentById_ReturnsMatchingProfile()
    {
        var c = new ServiceTestContext();
        var result = await c.StudentService.GetStudentById(c.Student.UserId);
        Assert.Equal(c.Student.UserId, result.UserId);
        Assert.Equal(c.Student.DisplayName, result.DisplayName);
        Assert.Equal(c.Topic.Id, Assert.Single(result.TopicInterests).Id);
    }

    [Fact]
    public async Task UpdateStudent_UpdatesFieldsAndPreservesIdentity()
    {
        var c = new ServiceTestContext();
        var request = c.StudentUpdate();
        var before = c.Student.UpdatedAt;
        var result = await c.StudentService.UpdateStudent(c.Student.UserId, request);
        Assert.Equal(c.Student.UserId, result.UserId);
        Assert.Equal(request.DisplayName, result.DisplayName);
        Assert.Equal(request.Program, result.Program);
        Assert.True(result.UpdatedAt >= before);
        Assert.Contains("UpdateStudent", c.Students.Calls);
    }

    [Fact]
    public async Task GetStudentById_ThrowsForMissingProfile()
    {
        var c = new ServiceTestContext();
        await ServiceFailureCases.AssertDomainFailure(() => c.StudentService.GetStudentById(Guid.NewGuid()), "GetStudentById", ErrorType.NotFound);
        Assert.DoesNotContain("UpdateStudent", c.Students.Calls);
    }

    [Fact]
    public async Task UpdateStudent_ThrowsForMissingProfile()
    {
        var c = new ServiceTestContext();
        await ServiceFailureCases.AssertDomainFailure(() => c.StudentService.UpdateStudent(Guid.NewGuid(), c.StudentUpdate()), "UpdateStudent", ErrorType.NotFound);
        Assert.DoesNotContain("UpdateStudent", c.Students.Calls);
    }

    [Fact]
    public async Task UpdateStudent_RejectsInvalidRequestBeforeWriting()
    {
        var c = new ServiceTestContext();
        var request = c.StudentUpdate();
        request.DisplayName = " ";
        await ServiceFailureCases.AssertDomainFailure(() => c.StudentService.UpdateStudent(c.Student.UserId, request), "UpdateStudent", ErrorType.Validation);
        Assert.DoesNotContain("UpdateStudent", c.Students.Calls);
    }


    [Fact]
    public void Constructor_DependsOnlyOnOwnRepository()
    {
        var constructor = Assert.Single(typeof(API.Services.StudentService).GetConstructors());
        var dependency = Assert.Single(constructor.GetParameters());
        Assert.Equal(typeof(API.Repositories.Interfaces.IStudentRepository), dependency.ParameterType);
    }

    [Theory]
    [MemberData(nameof(ServiceFailureCases.All), MemberType = typeof(ServiceFailureCases))]
    public async Task ValidateStudentExists_PreservesDependencyFailureAndClassification(string kind)
    {
        var c = new ServiceTestContext();
        using var cancellation = new CancellationTokenSource();
        var original = ServiceFailureCases.Create(kind);
        c.Students.Failure = original;
        await ServiceFailureCases.AssertWrapped(() => c.StudentService.ValidateStudentExists(c.Student.UserId, cancellation.Token), "ValidateStudentExists", original);
        Assert.Equal(cancellation.Token, c.Students.LastToken);
    }

    [Fact]
    public async Task ValidateStudentExists_AcceptsExistingReferences()
    {
        var c = new ServiceTestContext();
        await c.StudentService.ValidateStudentExists(c.Student.UserId);
        Assert.Equal("GetStudentById", Assert.Single(c.Students.Calls));
    }

    [Fact]
    public async Task ValidateStudentExists_RejectsMissingReferences()
    {
        var c = new ServiceTestContext();
        await ServiceFailureCases.AssertDomainFailure(() => c.StudentService.ValidateStudentExists(Guid.NewGuid()), "ValidateStudentExists", ErrorType.Validation);
    }
}
