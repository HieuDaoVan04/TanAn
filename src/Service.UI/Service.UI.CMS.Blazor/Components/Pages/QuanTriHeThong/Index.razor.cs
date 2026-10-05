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
using Service.UI.CMS.Blazor.Components.Layouts.ShareComponent.SecurePage;

namespace Service.UI.CMS.Blazor.Components.Pages.QuanTriHeThong;

public partial class Index
{
    [CascadingParameter] public CurrentUserDto CurrentUser { get; set; } = new();

    private sealed record AdminLink(string Title, string Description, string Path);
    private static readonly AdminLink[] Items =
    {
        new("Quản lý đơn vị/hệ thống", "Tổ chức cây đơn vị và hệ thống.", "/quan-tri-he-thong/don-vi-he-thong"),
        new("Quản lý người dùng", "Tạo tài khoản, đổi mật khẩu và gán vai trò.", "/quan-tri-he-thong/nguoi-dung"),
        new("Quản lý vai trò", "Khai báo vai trò và gán menu truy cập.", "/quan-tri-he-thong/vai-tro"),
        new("Tham số hệ thống", "Quản lý giá trị cấu hình.", "/quan-ly-tham-so-ht"),
        new("Quản lý menu", "Thiết kế cây menu và thứ tự hiển thị.", "/quan-tri-he-thong/quan-tri-menu"),
        new("Quản lý phiên", "Xem và thu hồi phiên đăng nhập Redis.", "/quan-tri-he-thong/quan-ly-phien")
    };

    private IEnumerable<AdminLink> AvailableItems => Items.Where(item =>
        MenuSecure.FindMenuId(CurrentUser, item.Path) is Guid id && MenuAccess.HasMenu(CurrentUser, id));

    protected override void OnParametersSet() => MenuAccess.Invalidate();
}
