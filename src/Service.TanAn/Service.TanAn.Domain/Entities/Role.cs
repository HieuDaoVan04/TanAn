// "Một sản phẩm của HieuDV"

using System;
using Service.Shared.Commons.Model.SQL;

namespace Service.TanAn.Domain.Entities
{
    public class Role
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string RoleName { get; set; } = string.Empty;
        public string RoleCode { get; set; } = string.Empty;
        public string? Mota { get; set; }
        public bool IsSync { get; set; }
        public DateTime Created { get; set; } = DateTime.UtcNow;
        public DateTime? LastModified { get; set; }
        public ModerationStatus ModerationStatus { get; set; } = ModerationStatus.Approved;
    }
}
