using uoProjects.Models;

namespace uoProjects.Services;

public class SavedProjectsService(HttpClient httpClient, NotificationService notifications) : HttpBaseService(httpClient, notifications)
{
    public Task<List<Project>?> ListAsync(Guid studentId)
    {
        return GetAsync<List<Project>>($"api/v1/students/{studentId}/saved-projects", "Could not load saved projects.");
    }

    public Task<Dictionary<Guid, int>?> CountsAsync()
    {
        return GetAsync<Dictionary<Guid, int>>("api/v1/saved-projects/counts", "Could not load save counts.");
    }
}
