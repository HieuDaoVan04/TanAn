namespace Service.TanAn.Domain.Entities;

/// <summary>Danh mục module chức năng; tách khỏi Module đang lưu cây menu.</summary>
public class PhanHe
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Ma { get; set; } = string.Empty;
    public string Ten { get; set; } = string.Empty;
    public string? MoTa { get; set; }
    public bool HoatDong { get; set; } = true;
}
