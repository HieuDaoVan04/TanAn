// "Một sản phẩm của HieuDV"

using System;
using Service.Shared.Commons.Model.SQL;
using Service.Shared.Commons.Models;

namespace Service.TanAn.Domain.Entities
{
    public class Module
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string TenModule { get; set; } = string.Empty;
        public string? Icon { get; set; }
        public string? LienKet { get; set; }
        public bool Expands { get; set; }
        public int ViTri { get; set; }
        public EnumModuleType PhanLoai { get; set; }
        public Guid? ModuleChaId { get; set; }
        public Module? ModuleCha { get; set; }
        public Guid? PhanHeId { get; set; }
        public PhanHe? PhanHe { get; set; }
        public ModerationStatus ModerationStatus { get; set; } = ModerationStatus.Approved;
    }
}
