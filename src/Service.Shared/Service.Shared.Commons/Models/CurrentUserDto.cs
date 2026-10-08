// "Một sản phẩm của HieuDV"

using System;
using System.Collections.Generic;

namespace Service.Shared.Commons.Models
{
    /// <summary>
    /// Thông tin người dùng hiện tại đang đăng nhập hệ thống
    /// Tác giả: HieuDV Pattern
    /// </summary>
    public class CurrentUserDto
    {
        public Guid UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string UserName { get => Username; set => Username = value; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        /// <summary>Mã các vai trò đã duyệt được gán cho tài khoản.</summary>
        public List<string> RoleCodes { get; set; } = new();
        public List<Guid> VillageIds { get; set; } = new();
        public string ApThon { get; set; } = string.Empty;
        public Guid? DepartmentId { get; set; }
        public string Token { get; set; } = string.Empty;
        public bool IsAuthenticated { get; set; }
        public List<SitePermissionDto> PhanQuyen { get; set; } = new List<SitePermissionDto>();
        public List<MenuItemDto> Menus { get; set; } = new List<MenuItemDto>();
        /// <summary>Cùng nguồn dữ liệu với Menus, tương thích cấu trúc SecurePage mẫu.</summary>
        public List<MenuItemDto> MenusActive
        {
            get => Menus;
            set => Menus = value ?? new List<MenuItemDto>();
        }
        public bool IsSuperUser => Role == "Admin" || Role == "SuperAdmin" || string.Equals(Username, "admin", StringComparison.OrdinalIgnoreCase);
        public HashSet<string> PermissionCache { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    }
}
