using System;
using System.Collections.Generic;
using Service.TanAn.Domain.Enums;

namespace Service.TanAn.Domain.Entities
{
    public class DoiTuongAnSinh
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        public Guid NhanKhauId { get; set; }
        public NhanKhau? NhanKhau { get; set; }

        public DoiTuongAnSinhEnum LoaiDoiTuong { get; set; }
        public decimal MucTroCapHangThang { get; set; }
        public DateTime NgayBatDauHuong { get; set; } = DateTime.UtcNow;
        public bool TrangThaiHoatDong { get; set; } = true;
        public DateTime? NgayKetThucHuong { get; set; }
        public string? SoQuyetDinh { get; set; }
        public string? GhiChu { get; set; }

        public ICollection<LichSuTroCap> LichSuTroCaps { get; set; } = new List<LichSuTroCap>();
    }
}

