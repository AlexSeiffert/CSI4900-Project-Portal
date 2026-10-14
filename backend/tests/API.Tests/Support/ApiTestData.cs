using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using API.DTOs.Request;

namespace API.Tests.Support;

internal static class ApiTestData
{
    internal static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false) }
    };

    internal static CreateProject NewProject(Guid? supervisorId = null, List<Guid>? topicIds = null)
    {
        return new CreateProject
        {
            SupervisorId = supervisorId ?? PortalApiFactory.SupervisorId,
            Title = "Portal research",
            Description = "Searchable DESCRIPTION",
            TopicIds = topicIds ?? [PortalApiFactory.TopicId]
        };
    }

    internal static async Task<T> Read<T>(HttpResponseMessage response, HttpStatusCode expected = HttpStatusCode.OK)
    {
        Assert.Equal(expected, response.StatusCode);
        var value = await response.Content.ReadFromJsonAsync<T>(JsonOptions);
        Assert.NotNull(value);
        return value;
    }

    internal static async Task<DTOs.Project> CreateProject(HttpClient client, CreateProject? request = null)
    {
        var response = await client.PostAsJsonAsync("/api/v1/projects", request ?? NewProject(), JsonOptions);
        return await Read<DTOs.Project>(response, HttpStatusCode.Created);
    }
}
