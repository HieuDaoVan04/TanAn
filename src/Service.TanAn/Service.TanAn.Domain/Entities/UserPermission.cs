// "Một sản phẩm của HieuDV"

using System;
using System.Text.Json.Serialization;

namespace Service.TanAn.Domain.Entities
{
    public class UserPermission
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        
        [JsonIgnore]
        public virtual User? User { get; set; }
        
        public Guid PermissionId { get; set; }
        
        [JsonIgnore]
        public virtual Permission? Permission { get; set; }
        
        public Guid PhongBanId { get; set; }
        
        [JsonIgnore]
        public virtual Groups? PhongBan { get; set; }

        public DateTime Created { get; set; } = DateTime.UtcNow;
    }
}
