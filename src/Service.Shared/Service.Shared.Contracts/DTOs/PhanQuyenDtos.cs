// "Một sản phẩm của HieuDV"

using System;
using System.Collections.Generic;

namespace Service.Shared.Contracts.DTOs
{
    public class VaiTroDto
    {
        public Guid Id { get; set; }
        public string TenVaiTro { get; set; } = string.Empty;
        public string VaiTroCode { get; set; } = string.Empty;
    }

    public class PhanQuyenNguoiDungDto
    {
        public Guid UserId { get; set; }
        public Guid ChuyenTrangId { get; set; }
        public List<Guid> ChuyenMucIds { get; set; } = new();
        public List<VaiTroDto> VaiTros { get; set; } = new();
    }

    public class CopyUserInfoDto
    {
        public Guid SourceUserId { get; set; }
        public Guid TargetUserId { get; set; }
    }

    public class GanVaiTroVaoNguoiDungDto
    {
        public Guid PhongBanId { get; set; }
        public List<Guid> RoleIds { get; set; } = new();
        public List<Guid> LstUserIds { get; set; } = new();
        public List<Guid> PermissionIds { get; set; } = new();
    }
}
