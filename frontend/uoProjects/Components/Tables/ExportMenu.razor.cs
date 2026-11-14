using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using uoProjects.Models;

namespace uoProjects.Components;

public partial class ExportMenu : IDisposable
{
    [Parameter, EditorRequired] public ExportHost Host { get; set; } = default!;

    private static readonly (string Value, string Label)[] FormatOptions = [("csv", "CSV"), ("excel", "Excel"), ("json", "JSON"), ("pdf", "PDF")];

    private static (string Value, string Label)[] RowOptions(IExportSource source, int all) =>
        [("filter", $"Current filter ({source.FilteredCount})"), ("all", $"All data ({all})")];

    private bool open;
    private bool busy;
    private string format = "csv";
    private string scope = "filter";
    private int sourceIndex;
    private string? error;

    private IExportSource? Current => Host.Sources.Count == 0 ? null : Host.Sources[Math.Min(sourceIndex, Host.Sources.Count - 1)];

    protected override void OnInitialized() => Host.Changed += OnChanged;

    public void Dispose() => Host.Changed -= OnChanged;

    private void OnChanged() => _ = InvokeAsync(StateHasChanged);

    private void OnKey(KeyboardEventArgs e)
    {
        if (e.Key == "Escape")
        {
            open = false;
        }
    }

    private async Task DownloadAsync()
    {
        if (busy || Current is not { Disabled: false } source)
        {
            return;
        }

        busy = true;
        error = null;
        try
        {
            await source.ExportAsync(format, scope);
            open = false;
        }
        catch (Exception e) when (e is JSException or InvalidOperationException)
        {
            error = "Couldn't create the file. Check your connection and try again.";
        }
        finally
        {
            busy = false;
        }
    }
}
