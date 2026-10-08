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

public partial class ViewThongBaoHT
{
    private List<NotificationItem> Notifications = new()
    {
        new NotificationItem
        {
            Title = "Cập nhật dữ liệu biến động dân cư Thôn 1 & Thôn 2",
            Content = "Hệ thống đã tự động đồng bộ 45 hồ sơ biến động nhân khẩu mới trong ngày.",
            TimeAgo = "10 phút trước",
            BorderColor = "#2563eb",
            IconColor = Color.Accent,
            Icon = new Icons.Regular.Size20.Info()
        },
        new NotificationItem
        {
            Title = "Cảnh báo AI phát hiện 3 hồ sơ đối tượng nghi trùng lặp",
            Content = "Hệ thống AI đường trùng vừa gắn cờ 3 nhân khẩu cần cán bộ rà soát đối chiếu.",
            TimeAgo = "1 giờ trước",
            BorderColor = "#f59e0b",
            IconColor = Color.Warning,
            Icon = new Icons.Regular.Size20.Warning()
        },
        new NotificationItem
        {
            Title = "Lịch tiếp công dân và xử lý Dịch vụ công điện tử tuần này",
            Content = "UBND Xã Tân An triển khai nhận hồ sơ trực tuyến cấp giấy xác nhận cư trú.",
            TimeAgo = "3 giờ trước",
            BorderColor = "#16a34a",
            IconColor = Color.Success,
            Icon = new Icons.Regular.Size20.CheckmarkCircle()
        }
    };

    private class NotificationItem
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string TimeAgo { get; set; } = string.Empty;
        public string BorderColor { get; set; } = "#2563eb";
        public Color IconColor { get; set; } = Color.Accent;
        public Icon Icon { get; set; } = new Icons.Regular.Size20.Info();
    }
}
