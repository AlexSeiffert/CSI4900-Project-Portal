using ProjectPortal.Web.Api;

namespace ProjectPortal.Web.Admin;

/// <summary>
/// How the admin pages change data: through the API, then reloading everything so every tab agrees.
/// In the sample-data preview there is no API, so reads fall back to the loaded data and saves are refused.
/// </summary>
public sealed class AdminActions(ApiClient? api, Func<Task> reload)
{
    public bool CanSave => api is not null;

    /// <summary>Fetches a single record (which carries its topics), or returns the fallback in the preview.</summary>
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

    /// <summary>Makes a change, then reloads. Returns null on success, otherwise the message to show.</summary>
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
