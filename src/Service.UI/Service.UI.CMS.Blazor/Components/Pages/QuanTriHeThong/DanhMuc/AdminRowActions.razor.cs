using Microsoft.AspNetCore.Components;
using Service.Shared.Contracts.DTOs;
namespace Service.UI.CMS.Blazor.Components.Pages.QuanTriHeThong.DanhMuc;
public partial class AdminRowActions
{
    [Parameter, EditorRequired] public AdminRecord Item { get; set; } = default!;
    [Parameter] public AdminCatalog Kind { get; set; }
    [Parameter] public bool Busy { get; set; }
    [Parameter] public EventCallback<AdminRecord> OnPopulation { get; set; }
    [Parameter] public EventCallback<AdminRecord> OnView { get; set; }
    [Parameter] public EventCallback<AdminRecord> OnEdit { get; set; }
    [Parameter] public EventCallback<AdminRecord> OnStatus { get; set; }
    [Parameter] public EventCallback<AdminRecord> OnDelete { get; set; }
}
