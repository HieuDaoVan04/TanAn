// "Một sản phẩm của HieuDV"

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using Service.Shared.Contracts.DTOs;

namespace Service.UI.CMS.Blazor.Applications.Dtos
{
    public class TreeViewItemDTO : ITreeViewItem
    {
        public string Id { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public string? Code { get; set; }
        public int Type { get; set; }
        public int ModerationStatus { get; set; }
        public object? Data { get; set; }
        public Icon? IconCollapsed { get; set; }
        public Icon? IconExpanded { get; set; }
        public bool Expanded { get; set; } = true;
        public bool Disabled { get; set; }
        public Func<TreeViewItemExpandedEventArgs, System.Threading.Tasks.Task>? OnExpandedAsync { get; set; }
        public IEnumerable<ITreeViewItem>? Items { get; set; }
    }

    public class EditOrUpdateParametersGroupDto
    {
        public Guid Id { get; set; }
        public bool IsEditMode { get; set; }
        public bool IsAddGroup { get; set; } = true;
        public GroupsDto? Object { get; set; }
        public EventCallback OnRefresh { get; set; }
    }

    public class GanVaiTroParametersDto
    {
        public Guid UserId { get; set; }
        public Guid PhongBanId { get; set; }
        public EventCallback OnRefresh { get; set; }
        public HashSet<(Guid UserId, Guid PhongBanId)> ListUser { get; set; } = new();
    }

    public class EditOrUpdateModuleParametersDto
    {
        public Guid ParameterGuid { get; set; }
        public Guid PhongBanId { get; set; }
        public EventCallback OnRefresh { get; set; }
    }

    public class ViewParametersDto
    {
        public Guid Id { get; set; }
        public Guid GroupId { get; set; }
    }
}
