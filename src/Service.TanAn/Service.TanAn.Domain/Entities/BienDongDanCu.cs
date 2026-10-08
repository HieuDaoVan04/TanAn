using System;
using Service.TanAn.Domain.Enums;

namespace Service.TanAn.Domain.Entities
{
    public class BienDongDanCu
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public LoaiBienDongEnum LoaiBienDong { get; set; }
        
        public Guid NhanKhauId { get; set; }
        public NhanKhau? NhanKhau { get; set; }

        public DateTime NgayPhatSinh { get; set; } = DateTime.UtcNow;
        public string? NoiDenOrDi { get; set; }
        public string? NoiDi { get; set; }
        public string? NoiDen { get; set; }
        public Guid? CanBoId { get; set; }
        public User? CanBo { get; set; }
        public string LyDo { get; set; } = string.Empty;
        public string CanBoGhiNhan { get; set; } = string.Empty;
        public string? FileDinhKemUrl { get; set; }
        public string? HoSoKhaiSinhJson { get; set; }
        public DateTime NgayTao { get; set; } = DateTime.UtcNow;
    }
}

