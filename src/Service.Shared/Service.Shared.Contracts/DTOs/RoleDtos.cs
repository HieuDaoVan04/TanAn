// "Một sản phẩm của HieuDV"

using System;
using System.Collections.Generic;
using Service.Shared.Commons.Model.SQL;
using Service.Shared.Commons.Models;

namespace Service.Shared.Contracts.DTOs
{
    public class RoleDto : BaseEntiyDto
    {
        public int STT { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string RoleCode { get; set; } = string.Empty;
        public string? Mota { get; set; }
        public bool IsSync { get; set; }
        public bool DaGan { get; set; }
        public List<Guid>? IdPermissions { get; set; } = new();
    }

    public class RoleForm
    {
        public Guid Id { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string RoleCode { get; set; } = string.Empty;
        public string? Mota { get; set; }
    }

    public class RoleQuery : BaseQuery
    {
        public string? RoleName { get; set; }
        public string? RoleCode { get; set; }
        public int ModerationStatus { get; set; }
    }

    public class GanQuyenDto
    {
        public List<Guid>? LstIdPermission { get; set; }
    }

    public class GanMenuVaoVaiTroDto
    {
        public Guid RoleId { get; set; }
        public List<Guid>? ModuleIds { get; set; }
    }

    public class UserRoleHistoryDto
    {
        public int STT { get; set; }
        public Guid Id { get; set; }
        public List<string>? ListRoleBeforeEdit { get; set; }
        public List<string>? ListRoleAfterEdit { get; set; }
        public Guid UserId { get; set; }
        public Guid ChuyenTrangId { get; set; }
        public string ChuyenTrangName { get; set; } = string.Empty;
        public List<string>? LstChuyenMucAfterEdit { get; set; }
        public string? TenNguoiThucHien { get; set; }
        public List<string>? LstChuyenMucBeforeEdit { get; set; }
        public DateTime Created { get; set; }
    }
}
