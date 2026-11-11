using ProjectPortal.Web.Api;

namespace ProjectPortal.Web.Admin;

public sealed class AdminActions(ApiClient? api, Func<Task> reload)
{
    public bool CanSave => api is not null;

    public async Task<T?> GetAsync<T>(string path, T? fallback) where T : class
    {
        if (api is null)
        {
            return fallback;
        }
        try
        {
            return await api.GetAsync<T>(path);
        }
        catch (ApiException)
        {
            return fallback;
        }
    }

    public async Task<string?> SaveAsync(Func<ApiClient, Task> change)
    {
        if (api is null)
        {
            return "This is the sample-data preview, so changes aren't saved.";
        }
        try
        {
            await change(api);
            await reload();
            return null;
        }
        catch (ApiException e)
        {
            return e.Message;
        }
    }
}
