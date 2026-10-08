using System;
using System.Collections.Generic;
using Service.TanAn.Domain.Enums;

namespace Service.Shared.Contracts.DTOs
{
    public class DoiTuongAnSinhDto
    {
        public Guid Id { get; set; }
        public Guid NhanKhauId { get; set; }
        public string HoTen { get; set; } = string.Empty;
        public string CCCD { get; set; } = string.Empty;
        public string ApThon { get; set; } = string.Empty;
        public DoiTuongAnSinhEnum LoaiDoiTuong { get; set; }
        public decimal MucTroCapHangThang { get; set; }
        public DateTime NgayBatDauHuong { get; set; }
        public bool TrangThaiHoatDong { get; set; }
        public string? GhiChu { get; set; }
        public List<LichSuTroCapDto>? LichSuChiTra { get; set; }
    }

    public class CreateAnSinhForm
    {
        public Guid NhanKhauId { get; set; }
        public DoiTuongAnSinhEnum LoaiDoiTuong { get; set; }
        public decimal MucTroCapHangThang { get; set; }
        public DateTime NgayBatDauHuong { get; set; } = DateTime.UtcNow;
        public string? GhiChu { get; set; }
    }

    public class LichSuTroCapDto
    {
        public Guid Id { get; set; }
        public Guid DoiTuongAnSinhId { get; set; }
        public string ThangNam { get; set; } = string.Empty;
        public decimal SoTien { get; set; }
        public DateTime NgayChiTra { get; set; }
        public string NguoiChiTra { get; set; } = string.Empty;
        public string? GhiChu { get; set; }
    }

    public class CreateTroCapForm
    {
        public Guid DoiTuongAnSinhId { get; set; }
        public string ThangNam { get; set; } = string.Empty; // e.g. "09/2026"
        public decimal SoTien { get; set; }
        public string NguoiChiTra { get; set; } = "Cán bộ An sinh";
        public string? GhiChu { get; set; }
    }
}


