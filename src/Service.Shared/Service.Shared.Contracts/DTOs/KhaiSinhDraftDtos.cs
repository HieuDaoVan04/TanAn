using Service.Shared.Commons.Model.SQL;

namespace Service.Shared.Contracts.DTOs;

public class KhaiSinhDraftSaveForm
{
    public KhaiSinhForm HoSo { get; set; } = new();
    public Guid? ApThonId { get; set; }
    public int PhienBan { get; set; }
}

public class KhaiSinhDraftDto
{
    public Guid Id { get; set; }
    public string MaHoSo { get; set; } = "";
    public string HoTenTre { get; set; } = "";
    public string HoTenNguoiYeuCau { get; set; } = "";
    public string MaSoHo { get; set; } = "";
    public Guid? ApThonId { get; set; }
    public DateTime? NgaySinh { get; set; }
    public DateTime NgayTao { get; set; }
    public DateTime? NgaySua { get; set; }
    public int PhienBan { get; set; }
    public ModerationStatus ModerationStatus { get; set; } = ModerationStatus.Pending;
    public bool DaGhiNhan { get; set; }
    public string NguoiDuyet { get; set; } = "";
    public DateTime? NgayDuyet { get; set; }
    public KhaiSinhForm? HoSo { get; set; }
}

public class KhaiSinhApproveForm
{
    [System.ComponentModel.DataAnnotations.Range(1, int.MaxValue)]
    public int PhienBan { get; set; }
}
