using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.JSInterop;
using Microsoft.FluentUI.AspNetCore.Components;
using Orientation = Microsoft.FluentUI.AspNetCore.Components.Orientation;
using Align = Microsoft.FluentUI.AspNetCore.Components.Align;
using Color = Microsoft.FluentUI.AspNetCore.Components.Color;
using Icons = Microsoft.FluentUI.AspNetCore.Components.Icons;
using ApexCharts;
using Service.TanAn.Domain.Entities;
using Service.TanAn.Domain.Enums;
using Service.TanAn.Application.Interfaces;
using Service.Shared.Commons.Models;
using Service.Shared.Contracts.DTOs;
using Service.UI.CMS.Blazor;
using Service.UI.CMS.Blazor.Components;
using Service.UI.CMS.Blazor.Components.Shared;
using static Microsoft.AspNetCore.Components.Web.RenderMode;
using Service.UI.CMS.Blazor.Components.Layout;
using Service.UI.CMS.Blazor.Components.Shares.Cards;
using Service.UI.CMS.Blazor.Components.Shares.Filter;

namespace Service.UI.CMS.Blazor.Components.Shares.Filter;

public partial class SearchPanel
{
    [Parameter]
    public string Title { get; set; } = string.Empty;

    [Parameter]
    public string Class { get; set; } = string.Empty;

    [Parameter]
    public bool ShowAdvanced { get; set; }

    [Parameter]
    public EventCallback<bool> ShowAdvancedChanged { get; set; }

    [Parameter]
    public string ShowAdvancedText { get; set; } = "Nâng cao";

    [Parameter]
    public string HideAdvancedText { get; set; } = "Ẩn nâng cao";

    [Parameter, EditorRequired]
    public RenderFragment? BasicFields { get; set; }

    [Parameter]
    public RenderFragment? AdvancedFields { get; set; }

    [Parameter]
    public RenderFragment? Actions { get; set; }

    [Parameter]
    public RenderFragment? HeaderActionsContent { get; set; }

    [Parameter]
    public string ActionsClass { get; set; } = string.Empty;

    private async Task ToggleAdvanced()
    {
        ShowAdvanced = !ShowAdvanced;
        await ShowAdvancedChanged.InvokeAsync(ShowAdvanced);
    }
}
