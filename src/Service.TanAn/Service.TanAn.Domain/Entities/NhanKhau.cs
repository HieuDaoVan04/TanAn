using System;
using Service.TanAn.Domain.Enums;

namespace Service.TanAn.Domain.Entities
{
    public class NhanKhau
    {
        public Guid Id { get; set; } = Guid.NewGuid();
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
        public string QuanHeVoiChuHo { get; set; } = "Chủ hộ";
        
        public Guid MaHoGiaDinh { get; set; }
        public HoGiaDinh? HoGiaDinh { get; set; }

        public TrangThaiNhanKhauEnum TrangThai { get; set; } = TrangThaiNhanKhauEnum.DangCuTru;
        public DateTime NgayTao { get; set; } = DateTime.UtcNow;
        public string? GhiChu { get; set; }

        public ICollection<ThanhVienHo> LichSuHoGiaDinh { get; set; } = new List<ThanhVienHo>();
        public ICollection<BienDongDanCu> BienDongs { get; set; } = new List<BienDongDanCu>();
        public ICollection<DoiTuongAnSinh> ChinhSachAnSinh { get; set; } = new List<DoiTuongAnSinh>();
    }
}

