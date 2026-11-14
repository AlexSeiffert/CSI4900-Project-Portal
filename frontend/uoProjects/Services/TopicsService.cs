using uoProjects.Models;

namespace uoProjects.Services;

public class TopicsService(HttpClient httpClient, NotificationService notifications) : HttpBaseService(httpClient, notifications)
{
    public Task<List<Topic>?> ListAsync(CancellationToken cancellationToken = default)
    {
        return GetAsync<List<Topic>>("api/v1/topics", "Could not load topics.", cancellationToken);
    }
}
