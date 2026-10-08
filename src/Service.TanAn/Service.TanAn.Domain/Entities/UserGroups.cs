// "Một sản phẩm của HieuDV"

using System;

namespace Service.TanAn.Domain.Entities
{
    public class UserGroups
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UsersId { get; set; }
        public Guid PhongBanId { get; set; }
        public Guid GroupId { get; set; }
        public DateTime Created { get; set; } = DateTime.UtcNow;
    }
}
