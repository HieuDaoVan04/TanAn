using System.Collections.Generic;

namespace Service.Shared.Contracts.DTOs
{
    public class ThongKeTongQuanDto
    {
        public int TongSoHoGiaDinh { get; set; }
        public int TongSoNhanKhau { get; set; }
        public int SoNhanKhauNam { get; set; }
        public int SoNhanKhauNu { get; set; }
        public int TongSoHoNgheoCanNgheo { get; set; }
        public int TongDoiTuongAnSinh { get; set; }
        public int TongYeuCauChuaXuLy { get; set; }
        public int TongYeuCauDaPheDuyet { get; set; }
        public List<ThongKeApThonDto> ThongKeTheoAp { get; set; } = new List<ThongKeApThonDto>();
        public List<ThongKeBienDongDto> ThongKeBienDongThang { get; set; } = new List<ThongKeBienDongDto>();
    }

    public class ThongKeApThonDto
    {
        public string TenApThon { get; set; } = string.Empty;
        public int SoHoGiaDinh { get; set; }
        public int SoNhanKhau { get; set; }
        public int SoDoiTuongAnSinh { get; set; }
    }

    public class ThongKeBienDongDto
    {
        public string LoaiBienDong { get; set; } = string.Empty;
        public int SoLuong { get; set; }
    }
}

