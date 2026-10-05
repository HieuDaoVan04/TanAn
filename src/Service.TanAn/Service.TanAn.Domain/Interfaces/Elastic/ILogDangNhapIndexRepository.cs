// "Một sản phẩm của HieuDV"

using System.Threading.Tasks;

namespace Service.TanAn.Domain.Interfaces.Elastic
{
    public class LogDangNhapIndexQuery
    {
        public System.DateTime? SearchTuNgay { get; set; }
        public System.DateTime? SearchDenNgay { get; set; }
        public string? Username { get; set; }
    }

    public interface ILogDangNhapIndexRepository
    {
        Task<int> GetCountData(LogDangNhapIndexQuery query);
    }
}
