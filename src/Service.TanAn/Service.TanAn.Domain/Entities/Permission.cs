// "Một sản phẩm của HieuDV"

using System;
using Service.Shared.Commons.Model.SQL;
using Service.Shared.Contracts.DTOs;

namespace Service.TanAn.Domain.Entities
{
    public class Permission
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string PermissionName { get; set; } = string.Empty;
        public EnumPermissions PermissionCode { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime Created { get; set; } = DateTime.UtcNow;
        public bool IsSync { get; set; }
        public DateTime? LastModified { get; set; }
        public ModerationStatus ModerationStatus { get; set; } = ModerationStatus.Approved;
        public Guid? PermissionParentId { get; set; }
        public Permission? PermissionParent { get; set; }
    }
}
