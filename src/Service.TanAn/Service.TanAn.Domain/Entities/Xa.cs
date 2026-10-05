namespace Service.TanAn.Domain.Entities;
public class Xa : BaseEntity
{
    public string Ma { get; set; } = "";
    public string Ten { get; set; } = "";
    public ICollection<ApThon> Thons { get; set; } = new List<ApThon>();
}
