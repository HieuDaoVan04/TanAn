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

namespace Service.UI.CMS.Blazor.Components.Pages.BanLamViec;

public partial class ViewTacVuNhanh
{
    private void NavigateTo(string url)
    {
        NavManager.NavigateTo(url);
    }

    private List<QuickTaskItem> QuickTasks = new()
    {
        new QuickTaskItem
        {
            Title = "Thêm Mới Hộ Khẩu",
            Subtitle = "Lập sổ hộ khẩu & nhân khẩu mới",
            Category = "Quản lý dân cư",
            Url = "/ho-khau",
            IconColor = Color.Accent,
            Icon = new Icons.Regular.Size24.PeopleAdd()
        },
        new QuickTaskItem
        {
            Title = "Khai Báo Biến Động",
            Subtitle = "Tách/chuyển/đăng ký tạm trú",
            Category = "Biến động nhân khẩu",
            Url = "/bien-dong",
            IconColor = Color.Success,
            Icon = new Icons.Regular.Size24.PersonArrowRight()
        },
        new QuickTaskItem
        {
            Title = "Đăng Ký An Sinh",
            Subtitle = "Chi trả trợ cấp & hộ nghèo",
            Category = "An sinh xã hội",
            Url = "/an-sinh",
            IconColor = Color.Warning,
            Icon = new Icons.Regular.Size24.HeartPulse()
        },
        new QuickTaskItem
        {
            Title = "Dịch Vụ Công Trực Tuyến",
            Subtitle = "Duyệt hồ sơ DVC điện tử",
            Category = "Dịch vụ công",
            Url = "/dich-vu-cong",
            IconColor = Color.Accent,
            Icon = new Icons.Regular.Size24.DocumentCheckmark()
        },
        new QuickTaskItem
        {
            Title = "AI Tra Cứu Trợ Lý",
            Subtitle = "Hỏi đáp văn bản & quy trình",
            Category = "Trợ lý AI",
            Url = "/ai-chatbot",
            IconColor = Color.Custom,
            Icon = new Icons.Regular.Size24.Bot()
        },
        new QuickTaskItem
        {
            Title = "Kiểm Tra AI Đường Trùng",
            Subtitle = "Rà soát dữ liệu sai lệch",
            Category = "AI Anomaly",
            Url = "/ai-duong-trung",
            IconColor = Color.Error,
            Icon = new Icons.Regular.Size24.BrainCircuit()
        }
    };

    private class QuickTaskItem
    {
        public string Title { get; set; } = string.Empty;
        public string Subtitle { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public Color IconColor { get; set; } = Color.Accent;
        public Icon Icon { get; set; } = new Icons.Regular.Size24.Apps();
    }
}
