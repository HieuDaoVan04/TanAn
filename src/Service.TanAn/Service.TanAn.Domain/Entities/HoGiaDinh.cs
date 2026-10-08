using System;
using System.Collections.Generic;

namespace Service.TanAn.Domain.Entities
{
    public class HoGiaDinh
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string MaSoHo { get; set; } = string.Empty;
        public string TenChuHo { get; set; } = string.Empty;
        public string CCCDChuHo { get; set; } = string.Empty;
        public string DiaChi { get; set; } = string.Empty;
        public string ApThon { get; set; } = string.Empty; // Ấp Tân Thạnh, Ấp Tân Bình, Ấp Tân Hoà, Ấp Tân Định, Ấp Tân Phong
        public DateTime NgayTao { get; set; } = DateTime.UtcNow;
        public string? GhiChu { get; set; }

        public Guid? ApThonId { get; set; }
        public ApThon? DiaBan { get; set; }
        public ICollection<ThanhVienHo> LichSuThanhVien { get; set; } = new List<ThanhVienHo>();
        public ICollection<PhanLoaiHo> LichSuPhanLoai { get; set; } = new List<PhanLoaiHo>();

        public ICollection<NhanKhau> ThanhVien { get; set; } = new List<NhanKhau>();
    }
}

