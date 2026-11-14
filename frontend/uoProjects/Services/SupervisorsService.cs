using uoProjects.Models;

namespace uoProjects.Services;

public class SupervisorsService(HttpClient httpClient, NotificationService notifications) : HttpBaseService(httpClient, notifications)
{
    public Task<List<Supervisor>?> ListAsync(CancellationToken cancellationToken = default)
    {
        return GetAsync<List<Supervisor>>("api/v1/supervisors", "Could not load supervisors.", cancellationToken);
    }

    public Task<Supervisor?> GetAsync(Guid id)
    {
        return GetAsync<Supervisor>($"api/v1/supervisors/{id}", "Could not load the supervisor.");
    }

    public Task<Supervisor?> UpdateAsync(Guid id, UpdateSupervisor request)
    {
        return SendAsync<Supervisor>(HttpMethod.Put, $"api/v1/supervisors/{id}", request, "Could not save the supervisor.");
    }
}
