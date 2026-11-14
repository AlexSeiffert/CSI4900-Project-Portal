using uoProjects.Models;

namespace uoProjects.Services;

public class ProjectsService(HttpClient httpClient, NotificationService notifications) : HttpBaseService(httpClient, notifications)
{
    public Task<List<Project>?> GetProjectsAsync(CancellationToken cancellationToken = default)
    {
        return GetAsync<List<Project>>("api/v1/projects", "An error occurred when fetching projects.", cancellationToken);
    }

    public Task<ProjectPage?> GetPageAsync(ProjectPageQuery query, CancellationToken cancellationToken = default)
    {
        var path = $"api/v1/projects/page?page={query.Page}&pageSize={query.PageSize}&sort={query.Sort}&descending={query.Descending}";
        if (query.Search.Length > 0)
        {
            path += $"&search={Uri.EscapeDataString(query.Search)}";
        }
        if (query.TopicId is { } topic)
        {
            path += $"&topicId={topic}";
        }
        if (query.Status is { } status)
        {
            path += $"&status={status}";
        }

        return GetAsync<ProjectPage>(path, "Could not load the projects page.", cancellationToken);
    }

    public Task<List<Project>?> GetProjectsWithTopicsAsync()
    {
        return GetAsync<List<Project>>("api/v1/projects?includeTopics=true", "Could not load projects and topics.");
    }

    public Task<Project?> GetProjectAsync(Guid id)
    {
        return GetAsync<Project>($"api/v1/projects/{id}", "Could not load the project.");
    }

    public Task<Project?> CreateAsync(UpdateProject request)
    {
        return SendAsync<Project>(HttpMethod.Post, "api/v1/projects", request, "Could not create the project.");
    }

    public Task<Project?> UpdateAsync(Guid id, UpdateProject request)
    {
        return SendAsync<Project>(HttpMethod.Put, $"api/v1/projects/{id}", request, "Could not save the project.");
    }

    public Task<bool> DeleteAsync(Guid id)
    {
        return SendAsync(HttpMethod.Delete, $"api/v1/projects/{id}", "Could not delete the project.");
    }
}
