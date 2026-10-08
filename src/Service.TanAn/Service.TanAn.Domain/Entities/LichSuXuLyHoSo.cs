using Service.TanAn.Domain.Enums;

namespace Service.TanAn.Domain.Entities;

public class LichSuXuLyHoSo : BaseEntity
{
    public Guid YeuCauId { get; set; }
    public YeuCauNguoiDan YeuCau { get; set; } = null!;
    public Guid? NguoiXuLyId { get; set; }
    public User? NguoiXuLy { get; set; }
    // Null ở lần tiếp nhận đầu tiên; người xử lý có thể null với thao tác hệ thống.
    public TrangThaiHoSoEnum? TrangThaiCu { get; set; }
    public TrangThaiHoSoEnum TrangThaiMoi { get; set; }
    public string? GhiChu { get; set; }
}
