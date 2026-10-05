using System;
using Service.TanAn.Domain.Enums;

namespace Service.TanAn.Domain.Entities
{
    public class YeuCauNguoiDan
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string MaYeuCau { get; set; } = string.Empty;
        public string HoTenNguoiYeuCau { get; set; } = string.Empty;
        public string CCCDNguoiYeuCau { get; set; } = string.Empty;
        public string SoDienThoai { get; set; } = string.Empty;
        public string LoaiYeuCau { get; set; } = string.Empty; // e.g. Khai sinh, Tạm trú, An sinh, Xác nhận cư trú
        public string NoiDung { get; set; } = string.Empty;
        public TrangThaiHoSoEnum TrangThai { get; set; } = TrangThaiHoSoEnum.MoiTiepNhan;
        public string? CanBoXuLy { get; set; }
        public string? GhiChuCanBo { get; set; }
        public DateTime NgayGui { get; set; } = DateTime.UtcNow;
        public DateTime NgayCapNhat { get; set; } = DateTime.UtcNow;
        public Guid? ApThonId { get; set; }
        public ApThon? Thon { get; set; }
        public Guid? NguoiNopId { get; set; }
        public User? NguoiNop { get; set; }
        public Guid? CanBoXuLyId { get; set; }
        public User? NguoiXuLy { get; set; }
        public ICollection<LichSuXuLyHoSo> LichSuXuLy { get; set; } = new List<LichSuXuLyHoSo>();
        public ICollection<TepDinhKem> TepDinhKems { get; set; } = new List<TepDinhKem>();
    }
}

