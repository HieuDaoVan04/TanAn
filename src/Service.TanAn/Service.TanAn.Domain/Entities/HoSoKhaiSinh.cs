using Service.Shared.Commons.Model.SQL;

namespace Service.TanAn.Domain.Entities;

/// <summary>Hồ sơ khai sinh do cán bộ nhập và Chủ tịch xã duyệt.</summary>
public class HoSoKhaiSinh : BaseEntity
{
    public string MaHoSo { get; set; } = "";
    public string HoTenTre { get; set; } = "";
    public string HoTenNguoiYeuCau { get; set; } = "";
    public string MaSoHo { get; set; } = "";
    public DateTime? NgaySinh { get; set; }
    public Guid? HoGiaDinhId { get; set; }
    public Guid? ApThonId { get; set; }
    public string NoiDungJson { get; set; } = "";
    public int PhienBan { get; set; } = 1;
    public ModerationStatus ModerationStatus { get; set; } = ModerationStatus.Pending;
    public Guid? NguoiDuyetId { get; set; }
    public string NguoiDuyet { get; set; } = "";
    public DateTime? NgayDuyet { get; set; }
}
