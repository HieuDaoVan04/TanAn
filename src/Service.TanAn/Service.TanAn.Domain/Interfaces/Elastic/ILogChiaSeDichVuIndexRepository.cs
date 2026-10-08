// "Một sản phẩm của HieuDV"

using System.Threading.Tasks;

namespace Service.TanAn.Domain.Interfaces.Elastic
{
    public interface ILogChiaSeDichVuIndexRepository
    {
        Task<int> GetCountData(object query);
    }
}
