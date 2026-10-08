// "Một sản phẩm của HieuDV"

using System;
using System.Collections.Generic;

namespace Service.TanAn.Domain.Entities
{
    public class UserRoleHistory
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public Guid ChuyenTrangId { get; set; }
        public string ChuyenTrangName { get; set; } = string.Empty;
        public string? TenNguoiThucHien { get; set; }
        public List<string>? ListRoleBeforeEdit { get; set; }
        public List<string>? ListRoleAfterEdit { get; set; }
        public List<string>? LstChuyenMucBeforeEdit { get; set; }
        public List<string>? LstChuyenMucAfterEdit { get; set; }
        public DateTime Created { get; set; } = DateTime.UtcNow;
    }
}
