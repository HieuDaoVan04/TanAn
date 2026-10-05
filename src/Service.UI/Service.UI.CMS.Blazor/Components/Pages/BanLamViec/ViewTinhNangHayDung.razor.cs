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

public partial class ViewTinhNangHayDung
{
    private List<FeatureItem> Features = new()
    {
        new FeatureItem
        {
            Name = "Sổ Hộ Khẩu & Dân Cư",
            Description = "Tra cứu hộ gia đình & nhân khẩu",
            Url = "/ho-khau",
            IconColor = Color.Accent,
            Icon = new Icons.Regular.Size24.BookDatabase()
        },
        new FeatureItem
        {
            Name = "Biến Động Nhân Khẩu",
            Description = "Quản lý tạm trú, tạm vắng",
            Url = "/bien-dong",
            IconColor = Color.Success,
            Icon = new Icons.Regular.Size24.PersonSwap()
        },
        new FeatureItem
        {
            Name = "Hồ Sơ An Sinh Xã Hội",
            Description = "Trợ cấp xã hội & người có công",
            Url = "/an-sinh",
            IconColor = Color.Warning,
            Icon = new Icons.Regular.Size24.Ribbon()
        },
        new FeatureItem
        {
            Name = "Hồ Sơ Dịch Vụ Công",
            Description = "Xử lý thủ tục hành chính công",
            Url = "/dich-vu-cong",
            IconColor = Color.Accent,
            Icon = new Icons.Regular.Size24.ClipboardTask()
        },
        new FeatureItem
        {
            Name = "Trợ Lý AI Tra Cứu",
            Subtitle = "",
            Description = "Tra cứu thông tin thông minh",
            Url = "/ai-chatbot",
            IconColor = Color.Custom,
            Icon = new Icons.Regular.Size24.ChatSparkle()
        },
        new FeatureItem
        {
            Name = "Nhật Ký Hệ Thống",
            Description = "Theo dõi tác động & audit log",
            Url = "/audit-logs",
            IconColor = Color.Neutral,
            Icon = new Icons.Regular.Size24.History()
        }
    };

    private class FeatureItem
    {
        public string Name { get; set; } = string.Empty;
        public string Subtitle { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public Color IconColor { get; set; } = Color.Accent;
        public Icon Icon { get; set; } = new Icons.Regular.Size24.Apps();
    }
}
