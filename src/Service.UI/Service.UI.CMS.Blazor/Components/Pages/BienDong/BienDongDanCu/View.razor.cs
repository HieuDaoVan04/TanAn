using Microsoft.AspNetCore.Components;
using Service.Shared.Contracts.DTOs;
using Service.TanAn.Domain.Enums;

namespace Service.UI.CMS.Blazor.Components.Pages.BienDong.BienDongDanCu;

public partial class View
{
    [Parameter, EditorRequired] public BienDongDto Record { get; set; } = default!;
    [Parameter] public EventCallback Close { get; set; }
    private KhaiSinhForm Form => Record.HoSoKhaiSinh!;
    private static string Text(string? text) => string.IsNullOrWhiteSpace(text) ? "—" : text;
    private static string Date(DateTime? date) => date?.ToString("dd/MM/yyyy") ?? "—";
    private static string GenderLabel(GioiTinhEnum gender) => gender switch
    {
        GioiTinhEnum.Nam => "Nam", GioiTinhEnum.Nu => "Nữ", _ => "Khác"
    };
}
