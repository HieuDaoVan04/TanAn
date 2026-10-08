// "Một sản phẩm của HieuDV"

using System;

namespace Service.TanAn.Domain.Entities
{
    public class RoleModule
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid RoleId { get; set; }
        public Guid ModuleId { get; set; }
    }
}
