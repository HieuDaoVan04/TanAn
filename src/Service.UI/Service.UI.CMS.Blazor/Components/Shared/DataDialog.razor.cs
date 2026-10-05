using Microsoft.AspNetCore.Components;
namespace Service.UI.CMS.Blazor.Components.Shared;
public partial class DataDialog
{
    [Parameter] public string Title { get; set; } = "Chi tiết";
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public EventCallback Close { get; set; }
    [Parameter] public bool Busy { get; set; }
}
