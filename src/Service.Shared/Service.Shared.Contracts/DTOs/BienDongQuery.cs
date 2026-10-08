using Service.Shared.Commons.Models;

namespace Service.Shared.Contracts.DTOs;

public class BienDongQuery : BaseQuery
{
    public int? LoaiBienDong { get; set; }
    public DateTime? TuNgay { get; set; }
    public DateTime? DenNgay { get; set; }
    public Guid? ApThonId { get; set; }
}
