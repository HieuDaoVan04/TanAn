namespace Service.TanAn.Domain.Entities;

/// <summary>
/// Một giai đoạn cư trú trong hộ. Quan hệ hộ hiện tại vẫn nằm ở NhanKhau.MaHoGiaDinh
/// để tương thích; service phải cập nhật lịch sử và quan hệ hiện tại trong cùng giao dịch.
/// </summary>
public class ThanhVienHo : BaseEntity
{
    public Guid HoGiaDinhId { get; set; }
    public HoGiaDinh HoGiaDinh { get; set; } = null!;
    public Guid NhanKhauId { get; set; }
    public NhanKhau NhanKhau { get; set; } = null!;
    public string QuanHeVoiChuHo { get; set; } = string.Empty;
    public bool LaChuHo { get; set; }
    public DateOnly TuNgay { get; set; }
    public DateOnly? DenNgay { get; set; }
    public string? GhiChu { get; set; }
}
