namespace Service.TanAn.Domain.Entities;
public class PhuTrachThon
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public Guid ApThonId { get; set; }
    public ApThon Thon { get; set; } = null!;
}
