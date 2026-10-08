// "Một sản phẩm của HieuDV"

using System;

namespace Service.TanAn.Domain.Entities
{
    public class RolePermission
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid RoleId { get; set; }
        public Guid PermissionId { get; set; }
        public Permission? Permission { get; set; }
    }
}
