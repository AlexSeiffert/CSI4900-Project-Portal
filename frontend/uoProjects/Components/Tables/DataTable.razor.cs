using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using uoProjects.Models;

namespace uoProjects.Components;

public partial class DataTable<TItem> : IExportSource, IDisposable
{
    [Inject] private IJSRuntime JS { get; set; } = default!;

    [Parameter, EditorRequired] public IEnumerable<TItem> Items { get; set; } = [];
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public string Empty { get; set; } = "Nothing here yet.";
    [Parameter] public string? SortedBy { get; set; }
    [Parameter] public IEnumerable<TItem>? AllItems { get; set; }
    [Parameter] public string ExportName { get; set; } = "table";
    [Parameter] public EventCallback<(string Column, bool Descending)> SortChanged { get; set; }
    [Parameter] public Func<string, Task<IEnumerable<TItem>>>? LoadExportRows { get; set; }
    [Parameter] public int? FilteredTotal { get; set; }
    [Parameter] public int? AllTotal { get; set; }
    [Parameter] public int PageSize { get; set; }
    [Parameter] public object? PageResetKey { get; set; }
    [Parameter] public bool ExportDisabled { get; set; }
    [Parameter] public bool Exportable { get; set; } = true;

    [CascadingParameter] public ExportHost? Host { get; set; }

    string IExportSource.Name => ExportName.Replace('-', ' ') is { Length: > 0 } n ? char.ToUpper(n[0]) + n[1..] : "Table";
    bool IExportSource.Disabled => ExportDisabled;
    int IExportSource.FilteredCount => FilteredTotal ?? Items.Count();
    int? IExportSource.AllCount => AllTotal ?? AllItems?.Count();

    protected override void OnInitialized()
    {
        if (Exportable)
        {
            Host?.Add(this);
        }
    }

    private int pageIndex;
    private object? previousResetKey;
    private int ItemCount => Items.Count();
    private int PageCount => PageSize > 0 ? (Math.Max(1, ItemCount) - 1) / PageSize + 1 : 1;

    protected override void OnParametersSet()
    {
        if (!Equals(previousResetKey, PageResetKey))
        {
            pageIndex = 0;
            previousResetKey = PageResetKey;
        }
        pageIndex = Math.Min(pageIndex, PageCount - 1);
        Host?.Refresh();
    }

    private IEnumerable<TItem> VisibleItems()
    {
        var ordered = Ordered(Items);
        return PageSize > 0 ? ordered.Skip(pageIndex * PageSize).Take(PageSize) : ordered;
    }

    public void Dispose() => Host?.Remove(this);

    private readonly List<DataColumn<TItem>> columns = [];
    private DataColumn<TItem>? sortColumn;
    private bool descending;

    internal void Add(DataColumn<TItem> column)
    {
        columns.Add(column);
        if (column.Title == SortedBy)
        {
            sortColumn = column;
            descending = column.DescendingFirst;
        }
        StateHasChanged();
    }

    private Task SortOn(DataColumn<TItem> column)
    {
        pageIndex = 0;
        descending = column == sortColumn ? !descending : column.DescendingFirst;
        sortColumn = column;
        return SortChanged.InvokeAsync((column.Title, descending));
    }

    private IEnumerable<TItem> Ordered(IEnumerable<TItem> items)
    {
        if (SortChanged.HasDelegate || sortColumn?.SortBy is not { } key)
        {
            return items;
        }
        var comparer = Comparer<object?>.Create(Compare);
        return descending ? items.OrderByDescending(key, comparer) : items.OrderBy(key, comparer);
    }

    async Task IExportSource.ExportAsync(string format, string scope)
    {
        var exportColumns = columns.Where(c => c.ExportValue is not null).ToList();
        var source = LoadExportRows is not null
            ? await LoadExportRows(scope)
            : scope == "all" && AllItems is not null ? AllItems : Items;
        var rows = Ordered(source).Select(item => exportColumns.Select(c => ExportCell(c.ExportValue!(item))).ToArray()).ToArray();
        await JS.InvokeVoidAsync("tableExport.download", format, ExportName, exportColumns.Select(c => c.Title).ToArray(), rows);
    }

    private static object ExportCell(object? value) => value switch
    {
        null => "",
        bool b => b ? "Yes" : "No",
        Enum e => e.ToString(),
        DateTime d => d.ToString("yyyy-MM-dd HH:mm"),
        DateTimeOffset d => d.ToString("yyyy-MM-dd HH:mm"),
        _ => value,
    };

    private static int Compare(object? a, object? b) => (a, b) switch
    {
        (null, null) => 0,
        (null, _) => 1,
        (_, null) => -1,
        (string x, string y) => string.Compare(x, y, StringComparison.CurrentCultureIgnoreCase),
        _ => Comparer<object>.Default.Compare(a, b),
    };
}
