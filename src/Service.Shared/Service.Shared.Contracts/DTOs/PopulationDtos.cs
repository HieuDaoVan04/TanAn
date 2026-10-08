using System;
using System.Collections.Generic;
using Service.TanAn.Domain.Enums;

namespace Service.Shared.Contracts.DTOs
{
    public class HoGiaDinhDto
    {
        public Guid Id { get; set; }
        public string MaSoHo { get; set; } = string.Empty;
        public string TenChuHo { get; set; } = string.Empty;
        public string CCCDChuHo { get; set; } = string.Empty;
        public string DiaChi { get; set; } = string.Empty;
        public string ApThon { get; set; } = string.Empty;
        public DateTime NgayTao { get; set; }
        public string? GhiChu { get; set; }
        public int SoThanhVien { get; set; }
        public List<NhanKhauDto>? ThanhVien { get; set; }
    }

    public class CreateHoGiaDinhForm
    {
        public DateTime? NgaySinhChuHo { get; set; }
        public GioiTinhEnum GioiTinhChuHo { get; set; } = GioiTinhEnum.Nam;
        public string MaSoHo { get; set; } = string.Empty;
        public string TenChuHo { get; set; } = string.Empty;
        public string CCCDChuHo { get; set; } = string.Empty;
        public string DiaChi { get; set; } = string.Empty;
        public string ApThon { get; set; } = string.Empty;
        public string? GhiChu { get; set; }
    }

    public class NhanKhauDto
    {
        public string? GhiChu { get; set; }
        public Guid Id { get; set; }
        public string HoTen { get; set; } = string.Empty;
        public string CCCD { get; set; } = string.Empty;
        public DateTime NgaySinh { get; set; }
        public GioiTinhEnum GioiTinh { get; set; }
        public string DanToc { get; set; } = "Kinh";
        public string TonGiao { get; set; } = "Không";
        public string QueQuan { get; set; } = string.Empty;
        public string ThuongTru { get; set; } = string.Empty;
        public string? TamTru { get; set; }
        public string? NgheNghiep { get; set; }
        public string? TrinhDoHocVan { get; set; }
        public string QuanHeVoiChuHo { get; set; } = "Thành viên";
        public Guid MaHoGiaDinh { get; set; }
        public string? MaSoHo { get; set; }
        public string? TenChuHo { get; set; }
        public TrangThaiNhanKhauEnum TrangThai { get; set; }
        public DateTime NgayTao { get; set; }
    }

    public class CreateNhanKhauForm
    {
        public string HoTen { get; set; } = string.Empty;
        public string CCCD { get; set; } = string.Empty;
        public DateTime NgaySinh { get; set; }
        public GioiTinhEnum GioiTinh { get; set; } = GioiTinhEnum.Nam;
        public string DanToc { get; set; } = "Kinh";
        public string TonGiao { get; set; } = "Không";
        public string QueQuan { get; set; } = "Xã Tân An";
        public string ThuongTru { get; set; } = string.Empty;
        public string? TamTru { get; set; }
        public string? NgheNghiep { get; set; }
        public string? TrinhDoHocVan { get; set; }
        public string QuanHeVoiChuHo { get; set; } = "Thành viên";
        public Guid MaHoGiaDinh { get; set; }
        public string? GhiChu { get; set; }
    }

    public class BienDongDto
    {
        public KhaiSinhForm? HoSoKhaiSinh { get; set; }
        public Guid Id { get; set; }
        public LoaiBienDongEnum LoaiBienDong { get; set; }
        public Guid NhanKhauId { get; set; }
        public string HoTenNhanKhau { get; set; } = string.Empty;
        public string CCCDNhanKhau { get; set; } = string.Empty;
        public DateTime NgayPhatSinh { get; set; }
        public string? NoiDenOrDi { get; set; }
        public string LyDo { get; set; } = string.Empty;
        public string CanBoGhiNhan { get; set; } = string.Empty;
        public DateTime NgayTao { get; set; }
    }

    public class CreateBienDongForm
    {
        public LoaiBienDongEnum LoaiBienDong { get; set; }
        public Guid NhanKhauId { get; set; }
        public DateTime NgayPhatSinh { get; set; } = DateTime.UtcNow;
        public string? NoiDenOrDi { get; set; }
        public string LyDo { get; set; } = string.Empty;
        public string CanBoGhiNhan { get; set; } = string.Empty;
    }
}


