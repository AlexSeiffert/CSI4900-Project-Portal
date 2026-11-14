namespace uoProjects.Admin;

public sealed class AdminActions(Func<Task>? reload)
{
    public bool CanSave => reload is not null;

    public Task<T?> GetAsync<T>(Func<Task<T?>> read, T? sample) where T : class
    {
        return CanSave ? read() : Task.FromResult(sample);
    }

    public Task<string?> SaveAsync<T>(Func<Task<T?>> change) where T : class
    {
        return SaveAsync(async () => await change() is not null);
    }

    public async Task<string?> SaveAsync(Func<Task<bool>> change)
    {
        if (reload is null)
        {
            return "The sample preview is read-only.";
        }
        if (!await change())
        {
            return "Changes could not be saved.";
        }

        await reload();
        return null;
    }
}
