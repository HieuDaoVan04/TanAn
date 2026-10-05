using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
namespace Service.UI.CMS.Blazor.Components.Shared;
public partial class DataPaginator : IDisposable
{
    [Parameter, EditorRequired] public PaginationState State { get; set; } = default!;
    private PaginationState? subscribed;
    private int Pages => Math.Max(1, (int)Math.Ceiling((State.TotalItemCount ?? 0) / (double)State.ItemsPerPage));
    protected override void OnParametersSet()
    {
        if (subscribed == State) return;
        if (subscribed != null) subscribed.TotalItemCountChanged -= Changed;
        subscribed = State; subscribed.TotalItemCountChanged += Changed;
    }
    private void Changed(object? sender, int? total) => _ = InvokeAsync(StateHasChanged);
    private Task Go(int page) => State.SetCurrentPageIndexAsync(page);
    private async Task ChangeSize(ChangeEventArgs args)
    {
        if (!int.TryParse(args.Value?.ToString(), out var size) || size is not (10 or 20 or 50)) return;
        State.ItemsPerPage = size; await State.SetCurrentPageIndexAsync(0);
    }
    public void Dispose() { if (subscribed != null) subscribed.TotalItemCountChanged -= Changed; }
}
