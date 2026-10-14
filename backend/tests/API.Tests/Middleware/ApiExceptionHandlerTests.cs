using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using API.Tests.Support;
using static API.Tests.Support.ApiTestData;

namespace API.Tests.Middleware;

public class ApiExceptionHandlerTests
{
    [Theory]
    [InlineData("projects", "Project")]
    [InlineData("inquiries", "Inquiry")]
    [InlineData("students", "Student")]
    [InlineData("supervisors", "Supervisor")]
    public async Task MissingResourcesSurfaceAnErrorMessageAndTraceId(string resource, string name)
    {
        await using var factory = new PortalApiFactory();
        using var client = await factory.CreateSeededClient();
        var id = Guid.NewGuid();
        var path = $"/api/v1/{resource}/{id}";
        var response = await client.GetAsync(path);
        var problem = await ReadProblem(response, HttpStatusCode.NotFound, path);
        Assert.Equal("Resource not found", problem.GetProperty("title").GetString());
        Assert.Equal($"{name} '{id}' was not found.", problem.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task MissingSavedProjectOwnerSurfacesWhichResourceIsMissing()
    {
        await using var factory = new PortalApiFactory();
        using var client = await factory.CreateSeededClient();
        var id = Guid.NewGuid();
        var path = $"/api/v1/students/{id}/saved-projects";
        var problem = await ReadProblem(await client.GetAsync(path), HttpStatusCode.NotFound, path);
        Assert.Contains(id.ToString(), problem.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task UpdateAndDeleteFailuresReturnProblemDetails()
    {
        await using var factory = new PortalApiFactory();
        using var client = await factory.CreateSeededClient();
        var path = $"/api/v1/projects/{Guid.NewGuid()}";
        await ReadProblem(await client.PutAsJsonAsync(path, NewProject(), JsonOptions), HttpStatusCode.NotFound, path);
        await ReadProblem(await client.DeleteAsync(path), HttpStatusCode.NotFound, path);
    }

    [Fact]
    public async Task ValidationErrorsAreSurfacedEvenWhenAcceptExcludesJson()
    {
        await using var factory = new PortalApiFactory();
        using var client = await factory.CreateSeededClient();
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/projects")
        {
            Content = JsonContent.Create(NewProject(Guid.NewGuid()), options: JsonOptions)
        };
        request.Headers.Accept.ParseAdd("text/plain");
        var problem = await ReadProblem(await client.SendAsync(request), HttpStatusCode.BadRequest, "/api/v1/projects");
        Assert.Equal("Supervisor does not exist.", problem.GetProperty("errors").GetProperty("request")[0].GetString());
    }

    [Theory]
    [InlineData("unexpected", HttpStatusCode.InternalServerError)]
    [InlineData("concurrency", HttpStatusCode.Conflict)]
    [InlineData("unique", HttpStatusCode.Conflict)]
    [InlineData("foreign-key", HttpStatusCode.Conflict)]
    [InlineData("cancelled", (HttpStatusCode)499)]
    public async Task SaveFailuresAreClassifiedWithoutExposingInternalDetails(string kind, HttpStatusCode status)
    {
        var exception = kind switch
        {
            "cancelled" => new OperationCanceledException("Private internal details"),
            "concurrency" => new DbUpdateConcurrencyException("Private internal details"),
            "unique" => new DbUpdateException("Private internal details", new PostgresException(
                "Private database details", "ERROR", "ERROR", PostgresErrorCodes.UniqueViolation)),
            "foreign-key" => new DbUpdateException("Private internal details", new PostgresException(
                "Private database details", "ERROR", "ERROR", PostgresErrorCodes.ForeignKeyViolation)),
            _ => (Exception)new InvalidOperationException("Private internal details")
        };
        await using var factory = new FailingSaveFactory(exception);
        using var client = await factory.CreateSeededClient();
        var response = await client.PostAsJsonAsync("/api/v1/projects", NewProject(), JsonOptions);
        var problem = await ReadProblem(response, status, "/api/v1/projects");
        Assert.DoesNotContain("Private", problem.GetRawText());
        Assert.DoesNotContain("stackTrace", problem.GetRawText());
    }

    private static async Task<JsonElement> ReadProblem(HttpResponseMessage response, HttpStatusCode status, string path)
    {
        Assert.Equal(status, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var problem = document.RootElement.Clone();
        Assert.Equal((int)status, problem.GetProperty("status").GetInt32());
        Assert.Equal(path, problem.GetProperty("instance").GetString());
        Assert.False(string.IsNullOrWhiteSpace(problem.GetProperty("traceId").GetString()));
        return problem;
    }
}
