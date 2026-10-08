// "Một sản phẩm của HieuDV"

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.FluentUI.AspNetCore.Components;
using Service.Shared.Commons.Enums;
using Service.Shared.Commons.Interfaces;
using Service.Shared.Commons.Models;
using Service.Shared.Contracts.DTOs;
using Service.UI.CMS.Blazor.Applications;
using Service.UI.CMS.Blazor.Components.Layouts.ShareComponent.SecurePage;
using Icons = Microsoft.FluentUI.AspNetCore.Components.Icons;

namespace Service.UI.CMS.Blazor.Components.Layout
{
    public partial class NavMenu : IDisposable
    {
        [Inject] private NavigationManager Navigation { get; set; } = default!;
        [Inject] private ModuleTypeState TypeState { get; set; } = default!;
        [Inject] private MenuTreeService MenuTree { get; set; } = default!;
        [Inject] private IMenuAccessCache MenuAccess { get; set; } = default!;

        [CascadingParameter] protected CurrentUserDto CurrentUser { set; get; } = new();
        private List<NavItem> NavMenuItems = new List<NavItem>();
        private Assembly assembly = typeof(Icons.Regular.Size20).Assembly;
        private bool IsLoadingDataMenu { get; set; } = true;
        private Guid? _loadedForUser;
        private bool _loadedAuthenticated;
        private HashSet<Guid> _loadedMenuIds = new();
        private int _refreshVersion;
        private bool _disposed;

        protected override void OnInitialized()
        {
            TypeState.Changed += OnModuleTypeChanged;
            Navigation.LocationChanged += OnLocationChanged;
        }

        protected override Task OnParametersSetAsync()
        {
            var menuIds = CurrentUser.MenusActive?.Select(x => x.Id) ?? Enumerable.Empty<Guid>();
            if (_loadedForUser == CurrentUser.UserId
                && _loadedAuthenticated == CurrentUser.IsAuthenticated
                && _loadedMenuIds.SetEquals(menuIds))
                return Task.CompletedTask;

            return Refresh();
        }

        private bool mobileExpanded;
        private void OnLocationChanged(object? sender, LocationChangedEventArgs e) => _ = InvokeAsync(async () => { mobileExpanded = false; await Refresh(); });

        private void OnModuleTypeChanged() => _ = InvokeAsync(Refresh);

        private List<ModuleTreeDto> FilterMenusByUser(
            List<ModuleTreeDto> allMenus)
        {
            var result = new List<ModuleTreeDto>();

            foreach (var menu in allMenus)
            {
                var filteredChildren = FilterMenusByUser(menu.Children);

                if (MenuAccess.HasMenu(CurrentUser, menu.Id) || filteredChildren.Any())
                {
                    result.Add(new ModuleTreeDto
                    {
                        Id = menu.Id,
                        ModuleChaId = menu.ModuleChaId,
                        TenModule = menu.TenModule,
                        Icon = menu.Icon,
                        LienKet = menu.LienKet,
                        Expands = menu.Expands,
                        ViTri = menu.ViTri,
                        PhanLoaiMenu = menu.PhanLoaiMenu,
                        PhanLoai = menu.PhanLoai,
                        Name = menu.Name,
                        ModerationStatus = menu.ModerationStatus,
                        Checked = menu.Checked,
                        Children = filteredChildren
                    });
                }
            }

            return result.OrderBy(m => m.ViTri).ToList();
        }

        private async Task Refresh()
        {
            var version = ++_refreshVersion;
            var userId = CurrentUser.UserId;
            var authenticated = CurrentUser.IsAuthenticated;
            var menuIds = CurrentUser.MenusActive?.Select(x => x.Id).ToHashSet() ?? new HashSet<Guid>();
            MenuAccess.Invalidate();
            var menus = await MenuTree.GetPublishedAsync();
            if (_disposed || version != _refreshVersion) return;

            // Đọc sau await để giữ cả thao tác mở/đóng trong lúc đang tải menu.
            var expanded = new Dictionary<Guid, bool>();
            if (_loadedForUser == userId && _loadedAuthenticated == authenticated)
                CaptureExpansion(NavMenuItems, expanded);
            // Giữ toàn bộ cây theo quyền khi chuyển trang, kể cả trong khu quản trị.
            var navItems = BuildNavItems(FilterMenusByUser(menus));
            NavMenuItems = FilterNavItems(navItems);
            RestoreExpansion(NavMenuItems, expanded);
            _loadedForUser = userId;
            _loadedAuthenticated = authenticated;
            _loadedMenuIds = menuIds;

            IsLoadingDataMenu = false;
            await InvokeAsync(StateHasChanged);
        }

        private static void CaptureExpansion(IEnumerable<NavItem> items, Dictionary<Guid, bool> expanded)
        {
            foreach (var group in items.OfType<NavGroup>())
            {
                expanded[group.MenuId] = group.Expanded;
                CaptureExpansion(group.Children, expanded);
            }
        }

        private static void RestoreExpansion(IEnumerable<NavItem> items, IReadOnlyDictionary<Guid, bool> expanded)
        {
            foreach (var group in items.OfType<NavGroup>())
            {
                if (expanded.TryGetValue(group.MenuId, out var value)) group.Expanded = value;
                RestoreExpansion(group.Children, expanded);
            }
        }

        private List<NavItem> FilterNavItems(IEnumerable<NavItem> items)
        {
            var result = new List<NavItem>();
            foreach (var item in items)
            {
                if (item is NavGroup group)
                {
                    var children = FilterNavItems(group.Children);
                    if (children.Count > 0) result.Add(group with { Children = children });
                }
                else if (MenuAccess.HasMenu(CurrentUser, item.MenuId)) result.Add(item);
            }
            return result;
        }

        public void Dispose()
        {
            _disposed = true;
            _refreshVersion++;
            TypeState.Changed -= OnModuleTypeChanged;
            Navigation.LocationChanged -= OnLocationChanged;
        }

        private List<NavItem> BuildNavItems(List<ModuleTreeDto> modules)
        {
            var items = new List<NavItem>();

            foreach (var dto in modules.OrderBy(x => x.ViTri))
            {
                var icon = GetIconByName(dto.Icon);

                if (dto.Children != null && dto.Children.Any())
                {
                    items.Add(new NavGroup(
                        icon,
                        dto.TenModule,
                        dto.Expands,
                        "gap-sm",
                        BuildNavItems(dto.Children)
                    )
                    { STT = dto.ViTri, MenuId = dto.Id, Href = MenuAccess.HasMenu(CurrentUser, dto.Id) ? dto.LienKet : null });
                }
                else if (!string.IsNullOrWhiteSpace(dto.LienKet))
                {
                    items.Add(new NavLink(
                        dto.LienKet,
                        icon,
                        dto.TenModule,
                        new List<EnumRoles>()
                    )
                    { STT = dto.ViTri, MenuId = dto.Id });
                }
            }

            return items;
        }

        private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, Icon> IconCache = new();

        private Icon GetIconByName(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return new Icons.Regular.Size20.Circle();

            return IconCache.GetOrAdd(name, iconName =>
            {
                Type? type = assembly.GetType($"Microsoft.FluentUI.AspNetCore.Components.Icons.Regular.Size20+{iconName}");
                if (type is null)
                    return new Icons.Regular.Size20.Circle();

                Icon? icon = (Icon?)Activator.CreateInstance(type);
                return icon ?? new Icons.Regular.Size20.Circle();
            });
        }
    }

    public abstract record NavItem
    {
        public Guid MenuId { get; init; }
        public string Title { get; init; } = string.Empty;
        public string? Href { get; init; }
        public NavLinkMatch Match { get; init; } = NavLinkMatch.All;
        public Icon Icon { get; init; } = new Icons.Regular.Size20.Document();
        public List<EnumRoles> RoleAppLy { set; get; } = new List<EnumRoles>();
        public int STT { get; set; } = 0;
    }

    public record NavLink : NavItem
    {
        public object? Tag { get; set; }
        public NavLink(string? href, Icon icon, string title, List<EnumRoles> roleAppLy, NavLinkMatch match = NavLinkMatch.All)
        {
            Href = href;
            Icon = icon;
            Title = title;
            Match = match;
            RoleAppLy = roleAppLy;
        }
    }

    public record NavGroup : NavItem
    {
        public bool Expanded { get; set; }
        public string Gap { get; init; }
        public List<NavItem> Children { get; set; }

        public NavGroup(Icon icon, string title, bool expanded, string gap, List<NavItem> children)
        {
            Href = null;
            Icon = icon;
            Title = title;
            Expanded = expanded;
            Gap = gap;
            Children = children;
        }
    }
}
