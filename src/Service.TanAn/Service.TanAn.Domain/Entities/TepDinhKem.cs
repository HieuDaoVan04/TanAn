namespace Service.TanAn.Domain.Entities;

/// <summary>Metadata của tệp; không lưu nội dung nhị phân trong entity.</summary>
public class TepDinhKem : BaseEntity
{
    public Guid YeuCauId { get; set; }
    public YeuCauNguoiDan YeuCau { get; set; } = null!;
    public string TenTep { get; set; } = string.Empty;
    public string DuongDanLuu { get; set; } = string.Empty;
    public string LoaiTep { get; set; } = string.Empty;
    public long DungLuong { get; set; }
}
