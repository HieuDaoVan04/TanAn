// "Một sản phẩm của HieuDV"

using System;
using System.Collections.Generic;

namespace Service.Shared.Contracts.DTOs
{
    public class ModuleTreeDto
    {
        public Guid Id { get; set; }
        public Guid? ModuleChaId { get; set; }
        public string TenModule { get; set; } = string.Empty;
        public string? Icon { get; set; }
        public string? LienKet { get; set; }
        public bool Expands { get; set; } = true;
        public int ViTri { get; set; }
        public string? PhanLoaiMenu { get; set; }
        public object? PhanLoai { get; set; }
        public string? Name { get; set; }
        public object? ModerationStatus { get; set; }
        public bool Checked { get; set; }
        public List<ModuleTreeDto> Children { get; set; } = new List<ModuleTreeDto>();
    }
}
