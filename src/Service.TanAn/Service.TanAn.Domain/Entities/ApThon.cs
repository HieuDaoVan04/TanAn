namespace Service.TanAn.Domain.Entities;

public class ApThon : BaseEntity
{
    public string Ma { get; set; } = string.Empty;
    public string Ten { get; set; } = string.Empty;
    public Guid? XaId { get; set; }
    public Xa? Xa { get; set; }
    public Guid? GroupId { get; set; }
    public Groups? DonVi { get; set; }
    public bool DangHoatDong { get; set; } = true;
    public ICollection<HoGiaDinh> HoGiaDinhs { get; set; } = new List<HoGiaDinh>();
}
