namespace Service.TanAn.Domain.Entities;
public class ThongBaoThon : BaseEntity
{
    public Guid NguoiNhanId { get; set; }
    public User NguoiNhan { get; set; } = null!;
    public Guid ApThonId { get; set; }
    public ApThon Thon { get; set; } = null!;
    public string TieuDe { get; set; } = "";
    public string NoiDung { get; set; } = "";
    public DateTime? DaDocLuc { get; set; }
}
