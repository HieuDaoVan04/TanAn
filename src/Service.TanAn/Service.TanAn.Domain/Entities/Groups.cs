// "Một sản phẩm của HieuDV"

using System;

namespace Service.TanAn.Domain.Entities
{
    public class Groups
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string GroupCode { get; set; } = string.Empty;
        public string GroupName { get; set; } = string.Empty;
        public string Name { get => GroupName; set => GroupName = value; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public string? CreatedBy { get; set; }
        public int UnitType { get; set; }
        public Guid? ParentId { get; set; }
    }
}
