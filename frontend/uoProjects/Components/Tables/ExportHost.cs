namespace uoProjects.Components;

public interface IExportSource
{
    string Name { get; }
    bool Disabled { get; }
    int FilteredCount { get; }
    int? AllCount { get; }
    Task ExportAsync(string format, string scope);
}

public sealed class ExportHost
{
    private readonly List<IExportSource> sources = [];

    public IReadOnlyList<IExportSource> Sources => sources;
    public event Action? Changed;

    public void Add(IExportSource source)
    {
        sources.Add(source);
        Changed?.Invoke();
    }

    public void Remove(IExportSource source)
    {
        if (sources.Remove(source))
        {
            Changed?.Invoke();
        }
    }

    public void Refresh()
    {
        Changed?.Invoke();
    }
}
