// "Một sản phẩm của HieuDV"

using System;
using System.Collections.Generic;

namespace Service.Shared.Commons.Models
{
    public class SitePermissionDto
    {
        public Guid SiteId { get; set; }
        public string SiteName { get; set; } = string.Empty;
        public List<CategoryPermissionDto> Categories { get; set; } = new List<CategoryPermissionDto>();
        public List<RolePermissionDto> Roles { get; set; } = new List<RolePermissionDto>();
        public List<PermissionDto> Permissions { get; set; } = new List<PermissionDto>();
    }

    public class CategoryPermissionDto
    {
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
    }

    public class RolePermissionDto
    {
        public Guid RoleId { get; set; }
        public string RoldeCode { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
    }

    public class PermissionDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    public class MenuItemDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public string? Icon { get; set; }
        public int Order { get; set; }
    }

    public class LuotTruyCap
    {
        public DateTime Ngay { get; set; }
        public int SoLuong { get; set; }
    }

    public static class AppGuids
    {
        public static readonly Guid Beautiful = Guid.Parse("11111111-2222-3333-4444-555555555555");
    }
}
