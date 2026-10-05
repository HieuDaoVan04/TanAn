using System;
using Service.TanAn.Domain.Enums;

namespace Service.Shared.Contracts.DTOs
{
    public class YeuCauDto
    {
        public Guid Id { get; set; }
        public string MaYeuCau { get; set; } = string.Empty;
        public string HoTenNguoiYeuCau { get; set; } = string.Empty;
        public string CCCDNguoiYeuCau { get; set; } = string.Empty;
        public string SoDienThoai { get; set; } = string.Empty;
        public string LoaiYeuCau { get; set; } = string.Empty;
        public string NoiDung { get; set; } = string.Empty;
        public TrangThaiHoSoEnum TrangThai { get; set; }
        public string? CanBoXuLy { get; set; }
        public string? GhiChuCanBo { get; set; }
        public DateTime NgayGui { get; set; }
        public DateTime NgayCapNhat { get; set; }
    }

    public class CreateYeuCauForm
    {
        public Guid? ApThonId { get; set; }
        public string HoTenNguoiYeuCau { get; set; } = string.Empty;
        public string CCCDNguoiYeuCau { get; set; } = string.Empty;
        public string SoDienThoai { get; set; } = string.Empty;
        public string LoaiYeuCau { get; set; } = string.Empty;
        public string NoiDung { get; set; } = string.Empty;
    }

    public class UpdateYeuCauStatusForm
    {
        public Guid YeuCauId { get; set; }
        public TrangThaiHoSoEnum TrangThai { get; set; }
        public string CanBoXuLy { get; set; } = string.Empty;
        public string? GhiChuCanBo { get; set; }
    }
}


