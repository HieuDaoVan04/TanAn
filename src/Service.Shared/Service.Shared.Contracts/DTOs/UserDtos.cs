// "Một sản phẩm của HieuDV"

using System;
using System.Collections.Generic;
using Service.Shared.Commons.Model.SQL;
using Service.Shared.Commons.Models;

namespace Service.Shared.Contracts.DTOs
{
    public class UserDto : BaseEntiyDto
    {
        public int Index { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public DateTime? LockoutEnd { get; set; }
        public DateTime? LastLogin { get; set; }
        public string? LastLoginIp { get; set; }
        public int TotalLogin { get; set; }
        public int TotalLoginFaild { get; set; }
        public int PhanLoai { get; set; }
        public string KeyPublic { get; set; } = string.Empty;
        public string DonVitxt { get; set; } = string.Empty;
        public string PhongBanTxt { get; set; } = string.Empty;
        public string VaiTrotxt { get; set; } = string.Empty;
        public Guid GroupID { get; set; }
        public Guid PhongBanId { get; set; }
        public List<UserRoleHistoryDto> UserRoleHistory { get; set; } = new List<UserRoleHistoryDto>();
    }

    public class UsersForm
    {
        public Guid Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? PasswordHash { get; set; }
        public string? PasswordMoi { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public int PhanLoai { get; set; }
        public string? KeyPublic { get; set; }
    }

    public class UserQuery : BaseQuery
    {
        public Guid GroupId { get; set; }
        public bool IsLayThuocDonVi { get; set; }
        public int PhanLoai { get; set; } = -1;
    }
}
