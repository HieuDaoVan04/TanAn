// "Một sản phẩm của HieuDV"

using System;
using System.ComponentModel;

namespace Service.Shared.Contracts.DTOs
{
    [AttributeUsage(AttributeTargets.Field)]
    public class PermissionDescriptionAttribute : Attribute
    {
        public string Description { get; }
        public PermissionDescriptionAttribute(string description)
        {
            Description = description;
        }
    }

    public enum EnumPermissions
    {
        None = 0,

        [Description("Quản trị hệ thống")]
        [PermissionDescription("Quyền quản trị toàn bộ hệ thống")]
        SystemAdmin = 100,

        [Description("Quản lý người dùng")]
        [PermissionDescription("Xem, thêm, sửa, xóa người dùng")]
        UserManagement = 200,

        [Description("Quản lý danh mục")]
        [PermissionDescription("Quản lý các danh mục nghiệp vụ")]
        CategoryManagement = 300,

        [Description("Quản lý hồ sơ công dân")]
        [PermissionDescription("Quản lý hồ sơ người dân, hộ gia đình")]
        CitizenManagement = 400
    }

    public enum EnumThaoTac
    {
        [Description("Xem")]
        Xem = 1,
        [Description("Thêm mới")]
        ThemMoi = 2,
        [Description("Cập nhật")]
        CapNhat = 3,
        [Description("Xóa")]
        Xoa = 4,
        [Description("Duyệt")]
        Duyet = 5,
        [Description("Hủy duyệt")]
        HuyDuyet = 6,
        [Description("Đồng bộ")]
        DongBo = 7
    }

    public enum EnumModules
    {
        [Description("Danh sách người dùng")]
        DanhSachNguoiDung = 1,
        [Description("Quản lý quyền")]
        QuanLyQuyen = 2,
        [Description("Quản lý vai trò")]
        QuanLyVaiTro = 3
    }
}
