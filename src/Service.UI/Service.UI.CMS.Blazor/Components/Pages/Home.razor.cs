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

namespace Service.UI.CMS.Blazor.Components.Pages;

public partial class Home
{
    [Inject] private IDashboardService DashboardService { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
    [Inject] private IToastService ToastService { get; set; } = default!;

    private ThongKeTongQuanDto? stats;
    private List<ThongKeApThon> apData = new();
    private List<ThongKeBienDong> bienDongData = new();
    private bool loading = true;
    private bool isRefreshing = false;
    private DateTime lastUpdated = DateTime.Now;
    private string? loadError;

    private bool HasVillageData => apData.Any(a => a.SoHoGiaDinh > 0 || a.SoNhanKhau > 0 || a.SoDoiTuongAnSinh > 0);
    private bool HasMovementData => bienDongData.Any(a => a.SoLuong > 0);

    protected override async Task OnInitializedAsync()
    {
        await LoadDashboardDataAsync();
    }

    private async Task<bool> LoadDashboardDataAsync()
    {
        loadError = null;

        try
        {
            var res = await DashboardService.GetThongKeTongQuanAsync();
            if (res.Success && res.Data != null)
            {
                stats = res.Data;
                apData = stats.ThongKeTheoAp?.Select(a => new ThongKeApThon
                {
                    TenApThon = a.TenApThon,
                    SoNhanKhau = a.SoNhanKhau,
                    SoHoGiaDinh = a.SoHoGiaDinh,
                    SoDoiTuongAnSinh = a.SoDoiTuongAnSinh
                }).ToList() ?? new();

                bienDongData = stats.ThongKeBienDongThang?.Select(b => new ThongKeBienDong
                {
                    LoaiBienDong = b.LoaiBienDong,
                    SoLuong = b.SoLuong
                }).ToList() ?? new();

                lastUpdated = DateTime.Now;
                return true;
            }

            loadError = string.IsNullOrWhiteSpace(res.Message)
                ? "Không tải được dữ liệu tổng quan."
                : res.Message;
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            loadError = "Không tải được dữ liệu tổng quan. Vui lòng thử lại.";
            return false;
        }
        finally
        {
            loading = false;
            isRefreshing = false;
        }
    }

    private async Task RefreshDataAsync()
    {
        isRefreshing = true;
        var success = await LoadDashboardDataAsync();
        if (success)
        {
            ToastService.ShowSuccess("Dữ liệu điều hành đã được cập nhật mới nhất!");
        }
        else
        {
            ToastService.ShowError(loadError ?? "Không thể cập nhật dữ liệu tổng quan.");
        }
    }

    private void NavigateTo(string url)
    {
        Navigation.NavigateTo(url);
    }

    private async Task ExportCsvAsync()
    {
        var csv = new System.Text.StringBuilder();
        csv.AppendLine("BÁO CÁO THỐNG KÊ ĐIỀU HÀNH TỔNG QUAN - UBND XÃ TÂN AN");
        csv.AppendLine($"Thời điểm xuất: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
        csv.AppendLine();
        csv.AppendLine("1. CHỈ SỐ TỔNG HỢP TOÀN XÃ");
        csv.AppendLine($"Tổng số hộ gia đình,{stats?.TongSoHoGiaDinh}");
        csv.AppendLine($"Tổng số nhân khẩu,{stats?.TongSoNhanKhau}");
        csv.AppendLine($"Nhân khẩu Nam,{stats?.SoNhanKhauNam}");
        csv.AppendLine($"Nhân khẩu Nữ,{stats?.SoNhanKhauNu}");
        csv.AppendLine($"Tổng đối tượng an sinh,{stats?.TongDoiTuongAnSinh}");
        csv.AppendLine($"Số hộ nghèo & cận nghèo,{stats?.TongSoHoNgheoCanNgheo}");
        csv.AppendLine($"Hồ sơ dịch vụ công đang xử lý,{stats?.TongYeuCauChuaXuLy}");
        csv.AppendLine($"Hồ sơ dịch vụ công đã giải quyết,{stats?.TongYeuCauDaPheDuyet}");
        csv.AppendLine();
        csv.AppendLine("2. PHÂN BỐ DÂN CƯ THEO ĐỊA BÀN THÔN");
        csv.AppendLine("Tên thôn,Số Hộ Gia Đình,Số Nhân Khẩu,Bình Quân Nhân Khẩu/Hộ,Đối Tượng An Sinh");
        foreach (var item in apData)
        {
            double bq = item.SoHoGiaDinh > 0 ? Math.Round((double)item.SoNhanKhau / item.SoHoGiaDinh, 1) : 0;
            csv.AppendLine($"\"{item.TenApThon}\",{item.SoHoGiaDinh},{item.SoNhanKhau},{bq},{item.SoDoiTuongAnSinh}");
        }
        csv.AppendLine();
        csv.AppendLine("3. THỐNG KÊ BIẾN ĐỘNG DÂN CƯ");
        csv.AppendLine("Loại Biến Động,Số Lượng");
        foreach (var item in bienDongData)
        {
            csv.AppendLine($"\"{item.LoaiBienDong}\",{item.SoLuong}");
        }

        var bytes = System.Text.Encoding.UTF8.GetPreamble().Concat(System.Text.Encoding.UTF8.GetBytes(csv.ToString())).ToArray();
        var base64 = Convert.ToBase64String(bytes);
        await JSRuntime.InvokeVoidAsync("eval", $"const a = document.createElement('a'); a.href = 'data:text/csv;charset=utf-8;base64,{base64}'; a.download = 'BaoCaoDieuHanh_XaTanAn_{DateTime.Now:yyyyMMdd_HHmm}.csv'; a.click();");
    }

    public class ThongKeApThon
    {
        public string TenApThon { get; set; } = "";
        public int SoNhanKhau { get; set; }
        public int SoHoGiaDinh { get; set; }
        public int SoDoiTuongAnSinh { get; set; }
        public double BinhQuanNhanKhau => SoHoGiaDinh > 0 ? Math.Round((double)SoNhanKhau / SoHoGiaDinh, 1) : 0;
    }

    public class ThongKeBienDong
    {
        public string LoaiBienDong { get; set; } = "";
        public int SoLuong { get; set; }
    }
}
