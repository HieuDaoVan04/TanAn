using Service.TanAn.Domain.Enums;

namespace Service.TanAn.Domain.Entities;

public class PhanLoaiHo : BaseEntity
{
    public Guid HoGiaDinhId { get; set; }
    public HoGiaDinh HoGiaDinh { get; set; } = null!;
    public LoaiHoEnum LoaiHo { get; set; }
    public DateOnly TuNgay { get; set; }
    public DateOnly? DenNgay { get; set; }
    public string? SoQuyetDinh { get; set; }
    public string? GhiChu { get; set; }
}
