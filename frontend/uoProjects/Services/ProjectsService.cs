using uoProjects.Models;

namespace uoProjects.Services;

public class ProjectsService(HttpClient httpClient, NotificationService notifications) : HttpBaseService(httpClient, notifications)
{
    public Task<List<Project>?> GetProjectsAsync(CancellationToken cancellationToken = default)
    {
        return GetAsync<List<Project>>("api/v1/projects", "An error occurred when fetching projects.", cancellationToken);
    }
}
