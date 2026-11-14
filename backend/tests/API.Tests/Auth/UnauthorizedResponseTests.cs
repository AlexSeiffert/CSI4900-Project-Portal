using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace API.Tests.Auth;

// Checks that 401 responses from the auth layer use the same problem-details JSON as other errors.
public class UnauthorizedResponseTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public UnauthorizedResponseTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.WithWebHostBuilder(builder => builder.UseEnvironment("Development")).CreateClient();
    }

    [Fact]
    public async Task MissingToken_Returns401ProblemDetails()
    {
        var response = await _client.GetAsync("/api/v1/projects");

        await AssertProblem(response);
    }

    [Fact]
    public async Task BadToken_Returns401ProblemDetails()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/projects");
        request.Headers.Authorization = new("Bearer", "not-a-real-token");

        await AssertProblem(await _client.SendAsync(request));
    }

    [Theory]
    [InlineData("student", HttpStatusCode.Forbidden)]
    [InlineData("admin", HttpStatusCode.OK)]
    public async Task SaveCounts_RequiresAdminRole(string role, HttpStatusCode expected)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/saved-projects/counts");
        request.Headers.Add("X-Dev-User-Id", Guid.NewGuid().ToString());
        request.Headers.Add("X-Dev-User-Role", role);

        using var response = await _client.SendAsync(request);

        Assert.Equal(expected, response.StatusCode);
    }

    [Theory]
    [InlineData("student", "", HttpStatusCode.Forbidden)]
    [InlineData("admin", "?page=0", HttpStatusCode.BadRequest)]
    [InlineData("admin", "?pageSize=101", HttpStatusCode.BadRequest)]
    [InlineData("admin", "?sort=999", HttpStatusCode.BadRequest)]
    [InlineData("admin", "", HttpStatusCode.OK)]
    public async Task ProjectPage_ChecksRoleAndQuery(string role, string query, HttpStatusCode expected)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/projects/page" + query);
        request.Headers.Add("X-Dev-User-Id", Guid.NewGuid().ToString());
        request.Headers.Add("X-Dev-User-Role", role);

        using var response = await _client.SendAsync(request);

        Assert.Equal(expected, response.StatusCode);
    }

    private static async Task AssertProblem(HttpResponseMessage response)
    {
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(401, body.RootElement.GetProperty("status").GetInt32());
        Assert.False(string.IsNullOrEmpty(body.RootElement.GetProperty("traceId").GetString()));
    }
}
