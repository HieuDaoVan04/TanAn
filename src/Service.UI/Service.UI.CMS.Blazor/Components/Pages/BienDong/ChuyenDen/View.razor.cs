using Microsoft.AspNetCore.Components;
using Service.Shared.Contracts.DTOs;
using Service.TanAn.Domain.Enums;

namespace Service.UI.CMS.Blazor.Components.Pages.BienDong.ChuyenDen;

public partial class View
{
    [Parameter, EditorRequired] public BienDongDto Record { get; set; } = default!;
    [Parameter] public EventCallback Close { get; set; }

}
