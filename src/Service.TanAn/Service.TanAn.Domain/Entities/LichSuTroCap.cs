using System;

namespace Service.TanAn.Domain.Entities
{
    public class LichSuTroCap
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        public Guid DoiTuongAnSinhId { get; set; }
        public DoiTuongAnSinh? DoiTuongAnSinh { get; set; }

        public string ThangNam { get; set; } = string.Empty; // e.g. "09/2026"
        public decimal SoTien { get; set; }
        public DateTime NgayChiTra { get; set; } = DateTime.UtcNow;
        public string NguoiChiTra { get; set; } = "Cán bộ An sinh";
        public string? GhiChu { get; set; }
        public Guid? NguoiChiTraId { get; set; }
        public User? CanBoChiTra { get; set; }
    }
}

