namespace Service.TanAn.Domain.Entities;

/// <summary>Thông tin định danh và theo dõi thay đổi dùng chung cho entity nghiệp vụ.</summary>
public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime NgayTao { get; set; } = DateTime.UtcNow;
    public Guid? NguoiTaoId { get; set; }
    public DateTime? NgaySua { get; set; }
    public Guid? NguoiSuaId { get; set; }
}
