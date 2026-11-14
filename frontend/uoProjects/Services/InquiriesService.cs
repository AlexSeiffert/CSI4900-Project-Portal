using uoProjects.Models;

namespace uoProjects.Services;

public class InquiriesService(HttpClient httpClient, NotificationService notifications) : HttpBaseService(httpClient, notifications)
{
    public Task<List<Inquiry>?> ListAsync()
    {
        return GetAsync<List<Inquiry>>("api/v1/inquiries", "Could not load inquiries.");
    }
}
