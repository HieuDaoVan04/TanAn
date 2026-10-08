using Microsoft.AspNetCore.Components;
using Service.Shared.Contracts.DTOs;
using Service.TanAn.Domain.Enums;

namespace Service.UI.CMS.Blazor.Components.Pages.BienDong.KhaiSinh;

public partial class View
{
    [Parameter] public BienDongDto? Record { get; set; }
    [Parameter] public KhaiSinhDraftDto? Draft { get; set; }
    [Parameter] public EventCallback Close { get; set; }
    [Parameter] public bool ApprovalEnabled { get; set; }
    [Parameter] public EventCallback Approve { get; set; }
    [Parameter] public string? ApprovalError { get; set; }
    private KhaiSinhForm? Form => Draft?.HoSo ?? Record?.HoSoKhaiSinh;
    private static string Text(string? text) => string.IsNullOrWhiteSpace(text) ? "—" : text;
    private static string Date(DateTime? date) => date?.ToString("dd/MM/yyyy") ?? "—";
    private static string Residence(LoaiCuTruKhaiSinh value) => value switch { LoaiCuTruKhaiSinh.ThuongTru => "Thường trú", LoaiCuTruKhaiSinh.TamTru => "Tạm trú", LoaiCuTruKhaiSinh.DangSinhSong => "Nơi đang sinh sống", LoaiCuTruKhaiSinh.NuocNgoai => "Ở nước ngoài", _ => "Chưa xác định loại cư trú" };
    private static string GenderLabel(GioiTinhEnum gender) => gender switch
    {
        GioiTinhEnum.Nam => "Nam", GioiTinhEnum.Nu => "Nữ", _ => "Khác"
    };
}
