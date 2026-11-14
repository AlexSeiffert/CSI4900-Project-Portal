using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using uoProjects.Models;

namespace uoProjects.Components;

public partial class EChart : IAsyncDisposable
{
    [Inject] private IJSRuntime JS { get; set; } = default!;

    [Parameter, EditorRequired] public string Kind { get; set; } = "";
    [Parameter, EditorRequired] public object Data { get; set; } = default!;
    [Parameter, EditorRequired] public string Label { get; set; } = "";
    [Parameter] public int Height { get; set; } = 300;
    [Parameter] public string? Class { get; set; }

    private ElementReference host;
    private IJSObjectReference? module;
    private object? drawn;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (ReferenceEquals(drawn, Data))
        {
            return;
        }
        drawn = Data;
        module ??= await JS.InvokeAsync<IJSObjectReference>("import", "./js/charts.js");
        await module.InvokeVoidAsync(Kind, host, Data);
    }

    public async ValueTask DisposeAsync()
    {
        if (module is not null)
        {
            await module.InvokeVoidAsync("dispose", host);
            await module.DisposeAsync();
        }
    }
}
