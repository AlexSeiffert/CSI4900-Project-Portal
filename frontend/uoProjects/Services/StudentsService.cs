using uoProjects.Models;

namespace uoProjects.Services;

public class StudentsService(HttpClient httpClient, NotificationService notifications) : HttpBaseService(httpClient, notifications)
{
    public Task<List<Student>?> ListAsync()
    {
        return GetAsync<List<Student>>("api/v1/students", "Could not load students.");
    }

    public Task<Student?> GetAsync(Guid id)
    {
        return GetAsync<Student>($"api/v1/students/{id}", "Could not load the student.");
    }

    public Task<Student?> UpdateAsync(Guid id, UpdateStudent request)
    {
        return SendAsync<Student>(HttpMethod.Put, $"api/v1/students/{id}", request, "Could not save the student.");
    }
}
