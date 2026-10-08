using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
namespace Service.UI.CMS.Blazor.Components.Shared;
public partial class Paginator
{
[Parameter, EditorRequired] public PaginationState PaginationState { get; set; } = default!;

    [Parameter] public EventCallback<int> SelectedPageSizeChanged { get; set; }

    [Parameter] public string? Class { get; set; }

    [Parameter] public string? Style { get; set; }

    private string SelectedPageSize { get; set; } = "10";

    private List<Option<int>> itemsPerPageOptions = new()
    {
        new Option<int> { Value = 5, Text = "5" },
        new Option<int> { Value = 10, Text = "10" },
        new Option<int> { Value = 20, Text = "20" },
        new Option<int> { Value = 50, Text = "50" },
        new Option<int> { Value = 100, Text = "100" },
    };

    protected override async Task OnInitializedAsync()
    {
        if (PaginationState.ItemsPerPage > 100)
            PaginationState.ItemsPerPage = 10;
        SelectedPageSize = PaginationState.ItemsPerPage.ToString();
        await base.OnInitializedAsync();
    }

    private async Task OnPageSizeChanged(string val)
    {
        if (int.TryParse(val, out var intVal))
        {
            SelectedPageSize = val;
            PaginationState.ItemsPerPage = intVal;
            await PaginationState.SetCurrentPageIndexAsync(0);
            await SelectedPageSizeChanged.InvokeAsync(intVal);
        }
    }


}
