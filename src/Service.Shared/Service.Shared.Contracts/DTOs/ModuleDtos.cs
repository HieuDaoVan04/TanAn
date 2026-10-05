// "Một sản phẩm của HieuDV"

using System;
using Service.Shared.Commons.Model.SQL;
using Service.Shared.Commons.Models;

namespace Service.Shared.Contracts.DTOs
{
    public class ModuleDto : BaseEntiyDto
    {
        public string TenModule { get; set; } = string.Empty;
        public EnumModuleType PhanLoai { get; set; }
        public string? Icon { get; set; }
        public string? LienKet { get; set; }
        public bool Expands { get; set; }
        public int ViTri { get; set; }
        public Guid? ModuleChaId { get; set; }
        public bool DaGan { get; set; }
    }

    public class ModuleForm
    {
        public Guid Id { get; set; }
        public string TenModule { get; set; } = string.Empty;
        public EnumModuleType PhanLoai { get; set; }
        public string? Icon { get; set; }
        public string? LienKet { get; set; }
        public bool Expands { get; set; }
        public int ViTri { get; set; }
        public Guid? ModuleChaId { get; set; }
    }
}
